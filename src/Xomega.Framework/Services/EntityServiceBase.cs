// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.ServiceModel;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// A generic base class for all service implementation classes that use Entity Framework.
    /// It implements the base interface for all Xomega interfaces and provides common functionality
    /// for entity-based services.
    /// </summary>
    /// <typeparam name="T">The type of the object context class to use.</typeparam>
    public class EntityServiceBase<T> : IServiceBase, IDisposable where T : ObjectContext, new()
    {
        /// <summary>
        /// Triggers <see cref="ValueFormat.StartUp"/> method if called first.
        /// </summary>
        private static readonly ValueFormat fmt = ValueFormat.Internal;

        /// <summary>
        /// Instrumentation hook.
        /// </summary>
        static EntityServiceBase() {}

        /// <summary>
        /// The object context of the specified type that is accessible to the subclasses.
        /// </summary>
        protected T objCtx = new T();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public EntityServiceBase()
        {
        }


        #region Entity Keys

        /// <summary>
        /// A dictionary of temporary keys generated for the objects created through the service.
        /// The entity keys are hashed by their integer IDs, which are negative to distinguish them
        /// from the real keys.
        /// </summary>
        private Dictionary<int, EntityObject> tempKeys = new Dictionary<int, EntityObject>();

        /// <summary>
        /// Generates a temporary integer ID for the given entity object and caches it internally
        /// so that this entity could be found later on by the generated temporary ID.
        /// This way the service can create a new object in one call and return the temporary ID
        /// to the client, which the latter can use to subsequently update the new object
        /// in another call in the same session and then save all the changes at the end.
        /// The temporary IDs are negative to distinguish them from the real keys.
        /// </summary>
        /// <param name="obj">The newly created entity object.</param>
        /// <returns>A generated temporary ID for the entity.</returns>
        protected int TempKeyId(EntityObject obj)
        {
            // make the temporary key Id negative to prevent potential conflicts with real keys
            int hash = -Math.Abs(obj.GetHashCode());
            tempKeys[hash] = obj;
            return hash;
        }

        /// <summary>
        /// Looks up the entity key for the given type of objects by the specified integer key.
        /// The integer key could be a temporary key for new objects or a real key for existing objects.
        /// </summary>
        /// <param name="type">The type of objects to get the key of.</param>
        /// <param name="key">The integer key to get the entity key by.</param>
        /// <returns>The entity key for the given type of objects by the specified integer key.</returns>
        protected EntityKey GetEntityKey(Type type, int? key)
        {
            return GetEntityKey(type, key, key);
        }

        /// <summary>
        /// Looks up the entity key for the given type of objects by the specified temporary integer key
        /// or the real values of an existing key.
        /// </summary>
        /// <param name="type">The type of objects to get the key of.</param>
        /// <param name="tempKey">The temporary integer key for new objects. Pass 0 to ignore.</param>
        /// <param name="keyVals">One or more values of the real key for existing objects in the order,
        /// in which they are listed on the entity definition.</param>
        /// <returns>The entity key for the given type of objects by the specified temporary integer key
        /// or the real values of an existing key.</returns>
        protected EntityKey GetEntityKey(Type type, int? tempKey, params object[] keyVals)
        {
            EntityObject entObj = null;
            if (tempKey != null && tempKeys.TryGetValue(tempKey.Value, out entObj)) return entObj.EntityKey;
            string entName = (from a in type.GetCustomAttributes(typeof(EdmEntityTypeAttribute), true)
                select ((EdmEntityTypeAttribute)a).Name).FirstOrDefault();
            var pk = from p in type.GetProperties()
                     from a in p.GetCustomAttributes(typeof(EdmScalarPropertyAttribute), true)
                     where ((EdmScalarPropertyAttribute)a).EntityKeyProperty
                     select p.Name;
            EntityKey entKey = new EntityKey(string.Format("{0}.{1}", objCtx.DefaultContainerName, entName),
                ToEntityKeyMemberList(pk, keyVals));
            return entKey;
        }

        /// <summary>
        /// Joins an enumeration of key names and key values into an enumeration of EntityKeyMember structures.
        /// </summary>
        /// <param name="keyNames">An enumeration of key names.</param>
        /// <param name="keyVals">An array of key values that correspond to the key names.</param>
        /// <returns>An enumeration of EntityKeyMember structures joined from the given key names and key values.</returns>
        private IEnumerable<EntityKeyMember> ToEntityKeyMemberList(
            IEnumerable<string> keyNames, object[] keyVals)
        {
            IEnumerator<string> epk = keyNames.GetEnumerator();
            IEnumerator ekv = keyVals.GetEnumerator();
            while (epk.MoveNext() && ekv.MoveNext())
            {
                yield return new EntityKeyMember(epk.Current, ekv.Current);
            }
        }

        #endregion

        #region Disposing

        /// <summary>
        /// Disposes of the entity service and the underlying object context.
        /// </summary>
        public void Dispose()
        {
            if (objCtx != null) objCtx.Dispose();
        }

        /// <summary>
        /// An explicit call to end the service session to support custom session mechanism for http bindings
        /// in Silverlight 3. This will allow releasing the instance of the service object on the server.
        /// </summary>
        [OperationBehavior(ReleaseInstanceMode=ReleaseInstanceMode.AfterCall)]
        public virtual void EndSession()
        {
            if (OperationContext.Current != null)
                OperationContext.Current.OutgoingMessageProperties["ReleaseInstance"] = true;
        }

        #endregion

        #region Saving Changes

        /// <summary>
        /// Validates all modified validatable objects in the current context.
        /// This method is always called when saving changes, but can also be called independently.
        /// </summary>
        public virtual void Validate()
        {
            var objects = from obj in objCtx.ObjectStateManager.GetObjectStateEntries(
                EntityState.Added | EntityState.Deleted | EntityState.Modified)
                where obj.Entity is IValidatable
                select obj.Entity;
            foreach (IValidatable obj in objects) obj.Validate(true);
        }

        /// <summary>
        /// Validates and saves all changes that have been made during prior service calls in the same session.
        /// If there are any validation errors during saving of the changes than a fault will be raised
        /// with an error list that contains all the errors. A fault will also be raised if there are only
        /// validation warnings and the <c>suppressWarnings</c> flag is passed in as false. In this case
        /// the client can review the warnings and re-issue the service call with this flag set to true
        /// to proceed regardless of the warnings.
        /// </summary>
        /// <param name="suppressWarnings">True to save changes even if there are warnings,
        /// False to raise a fault if there are any warnings.</param>
        /// <returns>The number of objects that have been added, modified, or deleted in the current session.</returns>
        /// <seealso cref="System.Data.Objects.ObjectContext.SaveChanges()"/>
        public virtual int SaveChanges(bool suppressWarnings)
        {
            return SaveChanges(objCtx, suppressWarnings);
        }

        protected virtual int SaveChanges(ObjectContext objCtx, bool suppressWarnings)
        {
            try
            {
                Validate();
                ErrorList errList = ErrorList.Current;
                errList.AbortIfHasErrors();
                if (!suppressWarnings && errList.Errors.Count > 0)
                    throw new FaultException<ErrorList>(errList, "Warnings have been posted.");
                return objCtx.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion
    }
}
