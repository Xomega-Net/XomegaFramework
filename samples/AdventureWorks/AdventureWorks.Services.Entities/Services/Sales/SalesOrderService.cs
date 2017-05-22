//---------------------------------------------------------------------------------------------
// This file was AUTO-GENERATED by "Service Implementations" Xomega.Net generator.
//
// Manual CHANGES to this file WILL BE LOST when the code is regenerated
// unless they are placed between corresponding CUSTOM_CODE_START/CUSTOM_CODE_END lines.
//---------------------------------------------------------------------------------------------

using AdventureWorks.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Xomega.Framework;
using Xomega.Framework.Services;
// CUSTOM_CODE_START: add namespaces for custom code below
// CUSTOM_CODE_END

namespace AdventureWorks.Services.Entities
{
    public partial class SalesOrderService : BaseService, ISalesOrderService
    {
        protected AdventureWorksEntities ctx;

        public SalesOrderService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            ctx = serviceProvider.GetService<AdventureWorksEntities>();
            if (ctx == null) ctx = new AdventureWorksEntities();
        }

        public virtual SalesOrder_ReadOutput Read(int _salesOrderId)
        {
            // CUSTOM_CODE_START: add custom security checks for Read operation below
            // CUSTOM_CODE_END
            SalesOrder_ReadOutput res = new SalesOrder_ReadOutput();
            try
            {
                SalesOrder obj = ctx.SalesOrder.Find(_salesOrderId);
                if (obj == null)
                {
                    currentErrors.CriticalError(ErrorType.Data, "SalesOrder with id {0} not found", _salesOrderId);
                }
                ServiceUtil.CopyProperties(obj, res);
                // CUSTOM_CODE_START: populate the Customer output structure of Read operation below
                res.Customer = GetCustomerInfo(obj); // CUSTOM_CODE_END
                // CUSTOM_CODE_START: populate the Payment output structure of Read operation below
                res.Payment = GetPaymentInfo(obj); // CUSTOM_CODE_END
                // CUSTOM_CODE_START: populate the Sales output structure of Read operation below
                res.Sales = GetSalesInfo(obj); // CUSTOM_CODE_END
                // CUSTOM_CODE_START: add custom code for Read operation below
                // CUSTOM_CODE_END
            }
            catch (Exception e)
            {
                throw e;
            }
            return res;
        }

        public virtual SalesOrder_CreateOutput Create(SalesOrder_CreateInput _data)
        {
            // CUSTOM_CODE_START: add custom security checks for Create operation below
            // CUSTOM_CODE_END
            SalesOrder_CreateOutput res = new SalesOrder_CreateOutput();
            try
            {
                EntityState state = EntityState.Added;
                SalesOrder obj = new SalesOrder();
                var entry = ctx.Entry(obj);
                entry.State = state;
                entry.CurrentValues.SetValues(_data);
                // CUSTOM_CODE_START: use the Customer input parameter of Create operation below
                UpdateCustomer(obj, _data.Customer); // CUSTOM_CODE_END
                // CUSTOM_CODE_START: use the Payment input parameter of Create operation below
                UpdatePayment(obj, _data.Payment); // CUSTOM_CODE_END
                // CUSTOM_CODE_START: use the Sales input parameter of Create operation below
                UpdateSalesInfo(obj, _data.Sales); // CUSTOM_CODE_END
                // CUSTOM_CODE_START: add custom code for Create operation below
                obj.OrderDate = DateTime.Now;
                obj.ModifiedDate = DateTime.Now;
                obj.Rowguid = Guid.NewGuid();
                // CUSTOM_CODE_END
                currentErrors.AbortIfHasErrors();
                ctx.SaveChanges();
                ServiceUtil.CopyProperties(obj, res);
            }
            catch (Exception e)
            {
                throw e;
            }
            return res;
        }

        public virtual SalesOrder_UpdateOutput Update(int _salesOrderId, SalesOrder_UpdateInput_Data _data)
        {
            // CUSTOM_CODE_START: add custom security checks for Update operation below
            // CUSTOM_CODE_END
            SalesOrder_UpdateOutput res = new SalesOrder_UpdateOutput();
            try
            {
                SalesOrder obj = ctx.SalesOrder.Find(_salesOrderId);
                if (obj == null)
                {
                    currentErrors.CriticalError(ErrorType.Data, "SalesOrder with id {0} not found", _salesOrderId);
                }
                var entry = ctx.Entry(obj);
                entry.CurrentValues.SetValues(_data);
                // CUSTOM_CODE_START: use the Customer input parameter of Update operation below
                UpdateCustomer(obj, _data.Customer); // CUSTOM_CODE_END
                // CUSTOM_CODE_START: use the Payment input parameter of Update operation below
                UpdatePayment(obj, _data.Payment); // CUSTOM_CODE_END
                // CUSTOM_CODE_START: use the Sales input parameter of Update operation below
                UpdateSalesInfo(obj, _data.Sales); // CUSTOM_CODE_END
                // CUSTOM_CODE_START: add custom code for Update operation below
                obj.ModifiedDate = DateTime.Now;
                // CUSTOM_CODE_END
                currentErrors.AbortIfHasErrors();
                ctx.SaveChanges();
                ServiceUtil.CopyProperties(obj, res);
            }
            catch (Exception e)
            {
                throw e;
            }
            return res;
        }

        public virtual void Delete(int _salesOrderId)
        {
            // CUSTOM_CODE_START: add custom security checks for Delete operation below
            // CUSTOM_CODE_END
            try
            {
                EntityState state = EntityState.Deleted;
                SalesOrder obj = ctx.SalesOrder.Find(_salesOrderId);
                if (obj == null)
                {
                    currentErrors.CriticalError(ErrorType.Data, "SalesOrder with id {0} not found", _salesOrderId);
                }
                var entry = ctx.Entry(obj);
                entry.State = state;
                // CUSTOM_CODE_START: add custom code for Delete operation below
                // CUSTOM_CODE_END
                currentErrors.AbortIfHasErrors();
                ctx.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public virtual IEnumerable<SalesOrder_ReadListOutput> ReadList(SalesOrder_ReadListInput_Criteria _criteria)
        {
            // CUSTOM_CODE_START: add custom security checks for ReadList operation below
            if (!CurrentPrincipal.IsEmployee() && !CurrentPrincipal.IsIndividualCustomer() &&
                !CurrentPrincipal.IsStoreContact())
            {
                currentErrors.CriticalError(ErrorType.Security, "Operation is not allowed");
            }
            // CUSTOM_CODE_END
            IEnumerable<SalesOrder_ReadListOutput> res = null;
            try
            {
                var src = from obj in ctx.SalesOrder select obj;
                #region Source filter
                if (_criteria != null)
                {

                    // CUSTOM_CODE_START: add code for GlobalRegion criteria of ReadList operation below
                    if (_criteria.GlobalRegion != null)
                    {
                        src = src.Where(o => _criteria.GlobalRegion == o.TerritoryIdObject.Group);
                    } // CUSTOM_CODE_END
                }
                // CUSTOM_CODE_START: add custom filter criteria to the source query for ReadList operation below
                if (CurrentPrincipal.IsStoreContact())
                {
                    int? storeId = CurrentPrincipal.GetStoreId();
                    src = src.Where(o => o.CustomerIdObject.StoreIdObject.BusinessEntityId == storeId);
                }
                if (CurrentPrincipal.IsIndividualCustomer())
                {
                    int? personId = CurrentPrincipal.GetPersonId();
                    src = src.Where(o => o.CustomerIdObject.PersonIdObject.BusinessEntityId == personId);
                }
                // CUSTOM_CODE_END
                #endregion
                var qry = from obj in src
                          select new SalesOrder_ReadListOutput() {
                              SalesOrderId = obj.SalesOrderId,
                              SalesOrderNumber = obj.SalesOrderNumber,
                              Status = obj.Status,
                              OrderDate = obj.OrderDate,
                              ShipDate = obj.ShipDate,
                              DueDate = obj.DueDate,
                              TotalDue = obj.TotalDue,
                              OnlineOrderFlag = obj.OnlineOrderFlag,
                              // CUSTOM_CODE_START: set the CustomerStore output parameter of ReadList operation below
                              CustomerStore = obj.CustomerIdObject.StoreIdObject.Name, // CUSTOM_CODE_END
                              // CUSTOM_CODE_START: set the CustomerName output parameter of ReadList operation below
                              CustomerName = obj.CustomerIdObject.PersonIdObject.LastName + ", " +
                                             obj.CustomerIdObject.PersonIdObject.FirstName, // CUSTOM_CODE_END
                              SalesPersonId = obj.SalesPersonIdObject.BusinessEntityId,
                              TerritoryId = obj.TerritoryIdObject.TerritoryId,
                          };
                #region Result filter
                if (_criteria != null)
                {
                    #region SalesOrderNumber
                    if (_criteria.SalesOrderNumberOperator != null)
                    {
                        switch (_criteria.SalesOrderNumberOperator)
                        {
                            case Operators.IsEqualTo:
                                qry = qry.Where(o => o.SalesOrderNumber == _criteria.SalesOrderNumber); break;
                            case Operators.IsNotEqualTo:
                                qry = qry.Where(o => o.SalesOrderNumber != _criteria.SalesOrderNumber); break;
                            case Operators.Contains:
                                qry = qry.Where(o => o.SalesOrderNumber.Contains(_criteria.SalesOrderNumber)); break;
                            case Operators.DoesNotContain:
                                qry = qry.Where(o => !o.SalesOrderNumber.Contains(_criteria.SalesOrderNumber)); break;
                            default:
                                currentErrors.AddValidationError("Unsupported operator {0} for the Sales Order Number.", _criteria.SalesOrderNumberOperator); break;
                        }
                    }
                    #endregion

                    #region Status
                    if (_criteria.StatusOperator != null)
                    {
                        switch (_criteria.StatusOperator)
                        {
                            case Operators.IsOneOf:
                                qry = qry.WhereIn(o => o.Status, _criteria.Status); break;
                            case Operators.IsNoneOf:
                                qry = qry.WhereNotIn(o => o.Status, _criteria.Status); break;
                            default:
                                currentErrors.AddValidationError("Unsupported operator {0} for the Status.", _criteria.StatusOperator); break;
                        }
                    }
                    #endregion

                    #region OrderDate
                    if (_criteria.OrderDateOperator != null)
                    {
                        switch (_criteria.OrderDateOperator)
                        {
                            case Operators.IsEqualTo:
                                qry = qry.Where(o => o.OrderDate == _criteria.OrderDate); break;
                            case Operators.IsNotEqualTo:
                                qry = qry.Where(o => o.OrderDate != _criteria.OrderDate); break;
                            case Operators.IsEarlierThan:
                                qry = qry.Where(o => o.OrderDate < _criteria.OrderDate); break;
                            case Operators.IsLaterThan:
                                qry = qry.Where(o => o.OrderDate > _criteria.OrderDate); break;
                            case Operators.IsBetween:
                                qry = qry.Where(o => o.OrderDate >= _criteria.OrderDate && o.OrderDate <= _criteria.OrderDate2); break;
                            default:
                                currentErrors.AddValidationError("Unsupported operator {0} for the Order Date.", _criteria.OrderDateOperator); break;
                        }
                    }
                    #endregion

                    #region DueDate
                    if (_criteria.DueDateOperator != null)
                    {
                        switch (_criteria.DueDateOperator)
                        {
                            case Operators.IsEqualTo:
                                qry = qry.Where(o => o.DueDate == _criteria.DueDate); break;
                            case Operators.IsNotEqualTo:
                                qry = qry.Where(o => o.DueDate != _criteria.DueDate); break;
                            case Operators.IsEarlierThan:
                                qry = qry.Where(o => o.DueDate < _criteria.DueDate); break;
                            case Operators.IsLaterThan:
                                qry = qry.Where(o => o.DueDate > _criteria.DueDate); break;
                            case Operators.IsBetween:
                                qry = qry.Where(o => o.DueDate >= _criteria.DueDate && o.DueDate <= _criteria.DueDate2); break;
                            default:
                                currentErrors.AddValidationError("Unsupported operator {0} for the Due Date.", _criteria.DueDateOperator); break;
                        }
                    }
                    #endregion

                    #region TotalDue
                    if (_criteria.TotalDueOperator != null)
                    {
                        switch (_criteria.TotalDueOperator)
                        {
                            case Operators.IsEqualTo:
                                qry = qry.Where(o => o.TotalDue == _criteria.TotalDue); break;
                            case Operators.IsNotEqualTo:
                                qry = qry.Where(o => o.TotalDue != _criteria.TotalDue); break;
                            case Operators.IsLessThan:
                                qry = qry.Where(o => o.TotalDue < _criteria.TotalDue); break;
                            case Operators.IsNotLessThan:
                                qry = qry.Where(o => o.TotalDue >= _criteria.TotalDue); break;
                            case Operators.IsGreaterThan:
                                qry = qry.Where(o => o.TotalDue > _criteria.TotalDue); break;
                            case Operators.IsNotGreaterThan:
                                qry = qry.Where(o => o.TotalDue <= _criteria.TotalDue); break;
                            case Operators.IsBetween:
                                qry = qry.Where(o => o.TotalDue >= _criteria.TotalDue && o.TotalDue <= _criteria.TotalDue2); break;
                            default:
                                currentErrors.AddValidationError("Unsupported operator {0} for the Total Due.", _criteria.TotalDueOperator); break;
                        }
                    }
                    #endregion

                    #region CustomerStore
                    if (_criteria.CustomerStoreOperator != null)
                    {
                        switch (_criteria.CustomerStoreOperator)
                        {
                            case Operators.IsNull:
                                qry = qry.Where(o => o.CustomerStore == null); break;
                            case Operators.IsNotNull:
                                qry = qry.Where(o => o.CustomerStore != null); break;
                            case Operators.IsEqualTo:
                                qry = qry.Where(o => o.CustomerStore == _criteria.CustomerStore); break;
                            case Operators.IsNotEqualTo:
                                qry = qry.Where(o => o.CustomerStore != _criteria.CustomerStore); break;
                            case Operators.Contains:
                                qry = qry.Where(o => o.CustomerStore.Contains(_criteria.CustomerStore)); break;
                            case Operators.DoesNotContain:
                                qry = qry.Where(o => !o.CustomerStore.Contains(_criteria.CustomerStore)); break;
                            default:
                                currentErrors.AddValidationError("Unsupported operator {0} for the Customer Store.", _criteria.CustomerStoreOperator); break;
                        }
                    }
                    #endregion

                    #region CustomerName
                    if (_criteria.CustomerNameOperator != null)
                    {
                        switch (_criteria.CustomerNameOperator)
                        {
                            case Operators.IsNull:
                                qry = qry.Where(o => o.CustomerName == null); break;
                            case Operators.IsNotNull:
                                qry = qry.Where(o => o.CustomerName != null); break;
                            case Operators.IsEqualTo:
                                qry = qry.Where(o => o.CustomerName == _criteria.CustomerName); break;
                            case Operators.IsNotEqualTo:
                                qry = qry.Where(o => o.CustomerName != _criteria.CustomerName); break;
                            case Operators.Contains:
                                qry = qry.Where(o => o.CustomerName.Contains(_criteria.CustomerName)); break;
                            case Operators.DoesNotContain:
                                qry = qry.Where(o => !o.CustomerName.Contains(_criteria.CustomerName)); break;
                            default:
                                currentErrors.AddValidationError("Unsupported operator {0} for the Customer Name.", _criteria.CustomerNameOperator); break;
                        }
                    }
                    #endregion

                    #region TerritoryId
                    if (_criteria.TerritoryIdOperator != null)
                    {
                        switch (_criteria.TerritoryIdOperator)
                        {
                            case Operators.IsNull:
                                qry = qry.Where(o => o.TerritoryId == null); break;
                            case Operators.IsNotNull:
                                qry = qry.Where(o => o.TerritoryId != null); break;
                            case Operators.IsEqualTo:
                                qry = qry.Where(o => o.TerritoryId == _criteria.TerritoryId); break;
                            case Operators.IsNotEqualTo:
                                qry = qry.Where(o => o.TerritoryId != _criteria.TerritoryId); break;
                            default:
                                currentErrors.AddValidationError("Unsupported operator {0} for the Territory Id.", _criteria.TerritoryIdOperator); break;
                        }
                    }
                    #endregion

                    #region SalesPersonId
                    if (_criteria.SalesPersonIdOperator != null)
                    {
                        switch (_criteria.SalesPersonIdOperator)
                        {
                            case Operators.IsNull:
                                qry = qry.Where(o => o.SalesPersonId == null); break;
                            case Operators.IsNotNull:
                                qry = qry.Where(o => o.SalesPersonId != null); break;
                            case Operators.IsOneOf:
                                qry = qry.WhereIn(o => o.SalesPersonId, _criteria.SalesPersonId); break;
                            case Operators.IsNoneOf:
                                qry = qry.WhereNotIn(o => o.SalesPersonId, _criteria.SalesPersonId); break;
                            default:
                                currentErrors.AddValidationError("Unsupported operator {0} for the Sales Person Id.", _criteria.SalesPersonIdOperator); break;
                        }
                    }
                    #endregion
                }
                // CUSTOM_CODE_START: add custom filter criteria to the result query for ReadList operation below
                // qry = qry.Where(o => o.FieldName == VALUE);
                // CUSTOM_CODE_END
                #endregion
                currentErrors.AbortIfHasErrors();
                res = qry.ToList();
            }
            catch (Exception e)
            {
                throw e;
            }
            return res;
        }

        public virtual SalesOrderDetail_ReadOutput Detail_Read(int _salesOrderDetailId)
        {
            // CUSTOM_CODE_START: add custom security checks for Detail_Read operation below
            // CUSTOM_CODE_END
            SalesOrderDetail_ReadOutput res = new SalesOrderDetail_ReadOutput();
            try
            {
                SalesOrderDetail obj = ctx.SalesOrderDetail.Find(_salesOrderDetailId);
                if (obj == null)
                {
                    currentErrors.CriticalError(ErrorType.Data, "SalesOrderDetail with id {0} not found", _salesOrderDetailId);
                }
                ServiceUtil.CopyProperties(obj, res);
                res.SalesOrderId = obj.SalesOrderObject.SalesOrderId;
                // CUSTOM_CODE_START: set the SpecialOfferId output field of Detail_Read operation below
                // TODO: res.SpecialOfferId = ???; // CUSTOM_CODE_END
                // CUSTOM_CODE_START: set the ProductId output field of Detail_Read operation below
                // TODO: res.ProductId = ???; // CUSTOM_CODE_END
                // CUSTOM_CODE_START: add custom code for Detail_Read operation below
                // CUSTOM_CODE_END
            }
            catch (Exception e)
            {
                throw e;
            }
            return res;
        }

        public virtual SalesOrderDetail_CreateOutput Detail_Create(int _salesOrderId, SalesOrderDetail_CreateInput_Data _data)
        {
            // CUSTOM_CODE_START: add custom security checks for Detail_Create operation below
            // CUSTOM_CODE_END
            SalesOrderDetail_CreateOutput res = new SalesOrderDetail_CreateOutput();
            try
            {
                EntityState state = EntityState.Added;
                SalesOrderDetail obj = new SalesOrderDetail();
                var entry = ctx.Entry(obj);
                entry.State = state;
                obj.SalesOrderObject = ctx.SalesOrder.Find(_salesOrderId);
                if (obj.SalesOrderObject == null)
                    currentErrors.AddValidationError("Invalid value {0} for parameter SalesOrderId. Cannot find the corresponding SalesOrder object.", _salesOrderId);
                entry.CurrentValues.SetValues(_data);
                // CUSTOM_CODE_START: use the SpecialOfferId input parameter of Detail_Create operation below
                // TODO: ??? = _data.SpecialOfferId; // CUSTOM_CODE_END
                // CUSTOM_CODE_START: use the ProductId input parameter of Detail_Create operation below
                // TODO: ??? = _data.ProductId; // CUSTOM_CODE_END
                // CUSTOM_CODE_START: add custom code for Detail_Create operation below
                // CUSTOM_CODE_END
                currentErrors.AbortIfHasErrors();
                ctx.SaveChanges();
                ServiceUtil.CopyProperties(obj, res);
            }
            catch (Exception e)
            {
                throw e;
            }
            return res;
        }

        public virtual void Detail_Update(int _salesOrderDetailId, SalesOrderDetail_UpdateInput_Data _data)
        {
            // CUSTOM_CODE_START: add custom security checks for Detail_Update operation below
            // CUSTOM_CODE_END
            try
            {
                SalesOrderDetail obj = ctx.SalesOrderDetail.Find(_salesOrderDetailId);
                if (obj == null)
                {
                    currentErrors.CriticalError(ErrorType.Data, "SalesOrderDetail with id {0} not found", _salesOrderDetailId);
                }
                var entry = ctx.Entry(obj);
                entry.CurrentValues.SetValues(_data);
                // CUSTOM_CODE_START: use the SpecialOfferId input parameter of Detail_Update operation below
                // TODO: ??? = _data.SpecialOfferId; // CUSTOM_CODE_END
                // CUSTOM_CODE_START: use the ProductId input parameter of Detail_Update operation below
                // TODO: ??? = _data.ProductId; // CUSTOM_CODE_END
                // CUSTOM_CODE_START: add custom code for Detail_Update operation below
                // CUSTOM_CODE_END
                currentErrors.AbortIfHasErrors();
                ctx.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public virtual void Detail_Delete(int _salesOrderDetailId)
        {
            // CUSTOM_CODE_START: add custom security checks for Detail_Delete operation below
            // CUSTOM_CODE_END
            try
            {
                EntityState state = EntityState.Deleted;
                SalesOrderDetail obj = ctx.SalesOrderDetail.Find(_salesOrderDetailId);
                if (obj == null)
                {
                    currentErrors.CriticalError(ErrorType.Data, "SalesOrderDetail with id {0} not found", _salesOrderDetailId);
                }
                var entry = ctx.Entry(obj);
                entry.State = state;
                // CUSTOM_CODE_START: add custom code for Detail_Delete operation below
                // CUSTOM_CODE_END
                currentErrors.AbortIfHasErrors();
                ctx.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public virtual IEnumerable<SalesOrderDetail_ReadListOutput> Detail_ReadList(int _salesOrderId)
        {
            // CUSTOM_CODE_START: add custom security checks for Detail_ReadList operation below
            // CUSTOM_CODE_END
            IEnumerable<SalesOrderDetail_ReadListOutput> res = null;
            try
            {
                var src = from obj in ctx.SalesOrderDetail
                          where obj.SalesOrderObject.SalesOrderId == _salesOrderId
                          select obj;
                #region Source filter
                if (true)
                {
                    // CUSTOM_CODE_START: add code for SalesOrderId criteria of Detail_ReadList operation below
                    if (_salesOrderId != null)
                    {
                        // TODO: src = src.Where(o => _salesOrderId == _salesOrderId);
                    } // CUSTOM_CODE_END
                }
                // CUSTOM_CODE_START: add custom filter criteria to the source query for Detail_ReadList operation below
                // src = src.Where(o => o.FieldName == VALUE);
                // CUSTOM_CODE_END
                #endregion
                var qry = from obj in src
                          select new SalesOrderDetail_ReadListOutput() {
                              SalesOrderDetailId = obj.SalesOrderDetailId,
                              // CUSTOM_CODE_START: set the Product output parameter of Detail_ReadList operation below
                              Product = obj.SpecialOfferProductObject.ProductId, // CUSTOM_CODE_END
                              OrderQty = obj.OrderQty,
                              UnitPrice = obj.UnitPrice,
                              UnitPriceDiscount = obj.UnitPriceDiscount,
                              // CUSTOM_CODE_START: set the SpecialOffer output parameter of Detail_ReadList operation below
                              SpecialOffer = obj.SpecialOfferProductObject.SpecialOfferId, // CUSTOM_CODE_END
                              LineTotal = obj.LineTotal,
                              CarrierTrackingNumber = obj.CarrierTrackingNumber,
                          };
                #region Result filter
                if (true)
                {
                }
                // CUSTOM_CODE_START: add custom filter criteria to the result query for Detail_ReadList operation below
                // qry = qry.Where(o => o.FieldName == VALUE);
                // CUSTOM_CODE_END
                #endregion
                currentErrors.AbortIfHasErrors();
                res = qry.ToList();
            }
            catch (Exception e)
            {
                throw e;
            }
            return res;
        }
    }
}