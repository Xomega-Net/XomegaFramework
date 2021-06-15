#if EF6
using System.Data.Entity;
#else
using Microsoft.EntityFrameworkCore;
#endif
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xomega.Framework;

namespace AdventureWorks.Services.Entities
{
    /// <summary>
    /// Entity Framework DbContext class extension to provide data validations using Xomega Framework
    /// </summary>
    public static class DbContextValidation
    {
        /// <summary>
        /// Finds an entity with the given primary key values from the context or the store as needed.
        /// If the entity doesn't exist then a critical error will be reported, which aborts the operation.
        /// </summary>
        /// <typeparam name="T">The type of entity to return.</typeparam>
        /// <param name="ctx">The DbContext to use.</param>
        /// <param name="errorList">The current error list to use for reporting errors.</param>
        /// <param name="keys">The values of the primary key for the entity to be found.</param>
        /// <returns>The entity found by the given primary keys.</returns>
        public static T FindEntity<T>(this DbContext ctx, ErrorList errorList, params object[] keys) where T : class
        {
            T entity = ctx.Set<T>()?.Find(keys);
            if (entity == null)
            {
                string error = keys.Length > 1 ? Messages.EntityNotFoundByKeys : Messages.EntityNotFoundByKey;
                errorList.CriticalError(ErrorType.Data, error, typeof(T).Name, string.Join<object>(", ", keys));
            }
            return entity;
        }

        /// <summary>
        /// Asynchronously finds an entity with the given primary key values from the context or the store as needed.
        /// If the entity doesn't exist then a critical error will be reported, which aborts the operation.
        /// </summary>
        /// <typeparam name="T">The type of entity to return.</typeparam>
        /// <param name="ctx">The DbContext to use.</param>
        /// <param name="errorList">The current error list to use for reporting errors.</param>
        /// <param name="keys">The values of the primary key for the entity to be found.</param>
        /// <returns>The entity found by the given primary keys.</returns>
        public static async Task<T> FindEntityAsync<T>(this DbContext ctx, ErrorList errorList, CancellationToken token,
                                                       params object[] keys) where T : class
        {
#if EF6
            T entity = await ctx.Set<T>().FindAsync(token, keys);
#else
            T entity = await ctx.Set<T>().FindAsync(keys, token);
#endif
            if (entity == null)
            {
                string error = keys.Length > 1 ? Messages.EntityNotFoundByKeys : Messages.EntityNotFoundByKey;
                errorList.CriticalError(ErrorType.Data, error, typeof(T).Name, string.Join<object>(", ", keys));
            }
            return entity;
        }

        public static async Task<T> FindEntityAsync<T>(this DbContext ctx, ErrorList errorList, params object[] keys) where T : class
            => await ctx.FindEntityAsync<T>(errorList, default, keys);

        /// <summary>
        /// Validates if the given primary key is unique, and reports an error if an object with this key already exists.
        /// </summary>
        /// <typeparam name="T">The type of entity to validate.</typeparam>
        /// <param name="ctx">The DbContext to use.</param>
        /// <param name="errorList">The current error list to use for reporting errors.</param>
        /// <param name="keys">The values of the primary key to validate.</param>
        public static void ValidateUniqueKey<T>(this DbContext ctx, ErrorList errorList, params object[] keys) where T : class
        {
            if (keys == null || keys.Length == 0 || keys.All(k => k == null)) return;
            T entity = ctx.Set<T>()?.Find(keys);
            if (entity != null)
            {
                string error = keys.Length > 1 ? Messages.EntityExistsWithKeys : Messages.EntityExistsWithKey;
                errorList.CriticalError(ErrorType.Concurrency, error, typeof(T).Name, string.Join<object>(", ", keys));
            }
        }

        /// <summary>
        /// Asynchronously validates if the given primary key is unique, and reports an error if an object with this key already exists.
        /// </summary>
        /// <typeparam name="T">The type of entity to validate.</typeparam>
        /// <param name="ctx">The DbContext to use.</param>
        /// <param name="errorList">The current error list to use for reporting errors.</param>
        /// <param name="keys">The values of the primary key to validate.</param>
        public static async Task ValidateUniqueKeyAsync<T>(this DbContext ctx, ErrorList errorList,
                                                           CancellationToken token, params object[] keys) where T : class
        {
            if (keys == null || keys.Length == 0 || keys.All(k => k == null)) return;
#if EF6
            T entity = await ctx.Set<T>().FindAsync(token, keys);
#else
            T entity = await ctx.Set<T>().FindAsync(keys, token);
#endif
            if (entity != null)
            {
                string error = keys.Length > 1 ? Messages.EntityExistsWithKeys : Messages.EntityExistsWithKey;
                errorList.CriticalError(ErrorType.Concurrency, error, typeof(T).Name, string.Join<object>(", ", keys));
            }
        }

        public static async Task ValidateUniqueKeyAsync<T>(this DbContext ctx, ErrorList errorList, params object[] keys) where T : class
            => await ctx.ValidateUniqueKeyAsync<T>(errorList, default, keys);

        /// <summary>
        /// Validates if an object with the given primary key exists in the store, and reports an error if not.
        /// </summary>
        /// <typeparam name="T">The type of entity to validate.</typeparam>
        /// <param name="ctx">The DbContext to use.</param>
        /// <param name="errorList">The current error list to use for reporting errors.</param>
        /// <param name="param">The name of the key input parameter(s) to use in the error.</param>
        /// <param name="keys">The values of the primary key to validate.</param>
        public static void ValidateKey<T>(this DbContext ctx, ErrorList errorList, string param, params object[] keys) where T : class
        {
            if (keys == null || keys.Length == 0 || keys.All(k => k == null)) return;
            T entity = ctx.Set<T>()?.Find(keys);
            if (entity == null)
            {
                string error = keys.Length > 1 ? Messages.InvalidForeignKeys : Messages.InvalidForeignKey;
                errorList.AddValidationError(error, string.Join<object>(", ", keys), param, typeof(T).Name);
            }
        }

        /// <summary>
        /// Asynchronously validates if an object with the given primary key exists in the store, and reports an error if not.
        /// </summary>
        /// <typeparam name="T">The type of entity to validate.</typeparam>
        /// <param name="ctx">The DbContext to use.</param>
        /// <param name="errorList">The current error list to use for reporting errors.</param>
        /// <param name="param">The name of the key input parameter(s) to use in the error.</param>
        /// <param name="keys">The values of the primary key to validate.</param>
        public static async Task ValidateKeyAsync<T>(this DbContext ctx, ErrorList errorList, CancellationToken token,
                                                     string param, params object[] keys) where T : class
        {
            if (keys == null || keys.Length == 0 || keys.All(k => k == null)) return;
#if EF6
            T entity = await ctx.Set<T>().FindAsync(token, keys);
#else
            T entity = await ctx.Set<T>().FindAsync(keys, token);
#endif
            if (entity == null)
            {
                string error = keys.Length > 1 ? Messages.InvalidForeignKeys : Messages.InvalidForeignKey;
                errorList.AddValidationError(error, string.Join<object>(", ", keys), param, typeof(T).Name);
            }
        }

        public static async Task ValidateKeyAsync<T>(this DbContext ctx, ErrorList errorList, string param, params object[] keys) where T : class
            => await ctx.ValidateKeyAsync<T>(errorList, default, param, keys);

#if EF6
        /// <summary>
        /// Adds validation errors from the DbContext to the supplied error list.
        /// </summary>
        /// <param name="ctx">The DbContext to use.</param>
        /// <param name="errorList">The error list to add the validation errors to.</param>
        public static void AddValidationErrors(this DbContext ctx, ErrorList errorList)
        {
            foreach (var ever in ctx.GetValidationErrors())
            {
                foreach (var ver in ever.ValidationErrors)
                    errorList.AddValidationError(ver.ErrorMessage);
            }
        }
#endif
    }
}
