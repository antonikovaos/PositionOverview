using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using LS.Client.CIL.Common;
using LS.Client.CIL.Contracts;
using LS.Client.CIL.Controls;
using LS.Client.CIL.Entities;
using LS.Client.CIL.Services;
using LS.Client.CIL.ViewModels;
using LS.Client.SO.Data;
using LS.Client.SO.Entities;
using static LS.Client.CIL.Controls.ReqDiagram;
using LS.Client.CIL;
using System.Collections.ObjectModel;
using DevExpress.Mvvm.DataAnnotations;

namespace LS.Client.SO.ViewModels.Forms
{
    [POCOViewModel]
    public class PositionOverviewVM : AbstractFormVM
    {
        #region | Fields |

        private static SODataAccess _dataAccess;
        private static ICustomerFilter _customerFilter;
        private static IOrderOverviewFilter _orderOverviewFilter;
        private static IItemFilter _itemFilter;
        private static IPositionsOverviewFilter _positionsOverviewFilter;
        private object _crutch;
        private static SOMainVM _moduleVM;

        private readonly ProvisionData _provData;

        #endregion

        #region |Property|

        public virtual bool ActiveCancelButton { get; set; }

        public virtual bool ActiveApproveButton { get; set; }

        public virtual bool AllowEdit { get; set; }

        public virtual ObservableCollection<ExpandoObject> PositionsOverview { get; set; }

        public virtual IList<Column> PositionColumns { get; set; }

        public virtual dynamic SelectedPosition { get; set; }

        public virtual object SelectedActiveCustomerStatus { get; set; }

        public virtual List<object> ActiveItems { get; set; }

        public virtual bool ShowDiagram { get; set; }

        public virtual bool ShowTabl { get; set; }

        public virtual ReqDataVMSO ProvDiagramData { get; set; }

        public virtual ReqDataDiagram DiagramData { get; set; }
        public virtual bool ShowReqDiagram { get; set; }
        public virtual dynamic CalendarItem { get; set; }

        public dynamic DefaultSize { get; set; }

        public virtual IList<ReqPosDataVMSO> ReqOverview { get; set; }

        #endregion

        #region |Constructor|

        public PositionOverviewVM(SODataAccess dataAccess,
            ICustomerFilter customerFilter,
            IOrderOverviewFilter orderOverviewFilter, IItemFilter itemFilter,
            IPositionsOverviewFilter positionsOverviewFilter,
            ProvisionData provisionData, SOMainVM moduleVM)
        {
            _dataAccess = dataAccess;
            _customerFilter = customerFilter;
            _orderOverviewFilter = orderOverviewFilter;
            _itemFilter = itemFilter;
            _positionsOverviewFilter = positionsOverviewFilter;

            _provData = provisionData;

            _moduleVM = moduleVM;

            DefaultSize = new ExpandoObject();
            DefaultSize.Width = 900d;
            DefaultSize.Height = 600d;

            ActiveItems = new List<object>()
            {
                "все",
                "активный",
                "не активный"
            };
            SelectedActiveCustomerStatus = ActiveItems[1];

            _dataAccess.OnLoaded += (s, a) =>
            {
            };
        }

        #endregion       

        protected override void UpdateCore()
        {
            ApplyFilter();
        }

        protected override void LoadedCore()
        {
            //_moduleVM.AllowEditItem = true;
            //_moduleVM.Loaded();
            //   UpdateCore();
        }

        protected override void UnloadedCore()
        {

        }

        public void ApplyFilter()
        {
            var qStatement = string.Empty;

            var filterValues = _orderOverviewFilter.FilterValues();

            var orderId = filterValues["OrderId"];
            if (!string.IsNullOrEmpty(orderId)) qStatement += $" and d.code like '{orderId}'";

            var contractId = filterValues["ContractId"];
            if (!string.IsNullOrEmpty(contractId)) qStatement += $" and z.code like '{contractId}'";

            var status = filterValues["status"];
            if (!string.IsNullOrEmpty(status)) qStatement += $" and d.stat_key in ({status})";


            filterValues = _customerFilter.FilterValues();

            var code = filterValues["Code"];
            if (!string.IsNullOrEmpty(code)) qStatement += $" and x.code like '{code}'";

            var namecust = filterValues["Name"];
            if (!string.IsNullOrEmpty(namecust)) qStatement += $" and x.name like '{namecust}'";

            var region = filterValues["Region"];
            if (!string.IsNullOrEmpty(region)) qStatement += $" and x.region like '{region}'";

            var groupcust = filterValues["group"];
            if (!string.IsNullOrEmpty(groupcust)) qStatement += $" and x.GROUP_key in ({groupcust})";

            filterValues = _positionsOverviewFilter.FilterValues();
            var posstatus = filterValues["status"];
            if (!string.IsNullOrEmpty(posstatus)) qStatement += $" and p.stat_key in ({posstatus})";

            var datefrom = filterValues["datefrom"];
            if (!string.IsNullOrEmpty(datefrom))
                qStatement += $" and p.plan_date >= '{datefrom}' ";

            var datebefore = filterValues["datebefore"];
            if (!string.IsNullOrEmpty(datebefore))
                qStatement += $" and p.plan_date <= '{datebefore}' ";

            var artQuery = string.Empty;

            filterValues = _itemFilter.FilterValues();

            var art = filterValues["art"];
            if (!string.IsNullOrEmpty(art)) artQuery += $" and i.art like '{art}'";

            var name = filterValues["name"];
            if (!string.IsNullOrEmpty(name)) artQuery += $" and i.name like '{name}'";

            var type = filterValues["type"];
            if (!string.IsNullOrEmpty(type)) artQuery += $" and i.type_key in ({type})";

            var mode = filterValues["mode"];
            if (!string.IsNullOrEmpty(mode)) artQuery += $" and i.mode_key in ({mode})";

            var classes = filterValues["class"];
            if (!string.IsNullOrEmpty(classes)) artQuery += $" and i.class_key in ({classes})";

            var group = filterValues["group"];
            if (!string.IsNullOrEmpty(group)) artQuery += $" and i.group_key in ({@group})";

            var family = filterValues["family"];
            if (!string.IsNullOrEmpty(family)) artQuery += $" and i.family_key in ({family})";

            var stat = filterValues["stat"];
            if (!string.IsNullOrEmpty(stat)) artQuery += $" and i.stat_yn in ({stat})";

            qStatement += artQuery;

            if (qStatement.StartsWith(" and"))
                qStatement = qStatement.Remove(0, 4);


            if (string.IsNullOrEmpty(qStatement))
                qStatement = "1=1";

            var posRet = _dataAccess.OverviewData.PositionsOverview(qStatement);

            if (PositionColumns == null)
                PositionColumns = posRet.Columns;

            PositionsOverview = posRet.Rows != null
                ? new ObservableCollection<ExpandoObject>(posRet.Rows)
                : new ObservableCollection<ExpandoObject>();

            if (PositionsOverview.Count() > 1000)
                MessageService.Warning("Частичная выборка", "Сработало ограничение выборки, используйте фильтры");
        }

        public void ApprovePosition()
        {
            var docPos = IoC.Instance.GetInstance<DocumentPosVM>();

            if (SelectedPosition != null)
            {
                UpdateFields(docPos, SelectedPosition);
            }
            var answer = MessageService.Question("Утверждение позиции заказа", "Вы собираетесь утвердить позицию заказа.\nВы уверены в этом?");
            if (answer != MessageBoxResult.Yes) return;

            _dataAccess.ApprovePosition(docPos);

            var row = _dataAccess.DocumentPosData.Get(docPos.Id);

            if (SelectedPosition != null)
            {
                SelectedPosition.DocPos_Id = row.Id;
                SelectedPosition.DocPos_StatKey = row.PosStatus;

                if (SelectedPosition != null && SelectedPosition.DocPos_StatKey == 55)
                {
                    ActiveApproveButton = false;
                    ActiveCancelButton = true;
                }
                if (SelectedPosition != null && SelectedPosition.DocPos_StatKey == 125)
                {
                    ActiveApproveButton = false;
                    ActiveCancelButton = true;
                }
            }
        }

        public void Cancel()
        {
            var docPos = IoC.Instance.GetInstance<DocumentPosVM>();

            UpdateFields(docPos, SelectedPosition);

            var answer = MessageService.Question("Отмена позиции заказа", "Вы собираетесь отменить позицию заказа.\nВы уверены в этом?");
            if (answer != MessageBoxResult.Yes) return;

            _dataAccess.CancelPosition(docPos);

            var row = _dataAccess.DocumentPosData.Get(docPos.Id);

            if (SelectedPosition != null)
            {
                SelectedPosition.DocPos_Id = row.Id;
                SelectedPosition.DocPos_StatKey = row.PosStatus;
                // SelectedPosition.PositionStatusName = row.Status.Name;

                if (SelectedPosition != null && SelectedPosition.DocPos_StatKey == 30) //55
                {
                    ActiveCancelButton = false;
                    ActiveApproveButton = true;
                }
                if (SelectedPosition != null && SelectedPosition.DocPos_StatKey == 60) //55
                {
                    ActiveCancelButton = false;
                    ActiveApproveButton = false;
                }
            }
        }

        public void CancelPosition()
        {
            if (SelectedPosition != null && SelectedPosition.DocPos_StatKey == 55) //55
            {
                ActiveCancelButton = true;
                Cancel();
            }
            else if (SelectedPosition != null && SelectedPosition.DocPos_StatKey == 125)   //125
            {
                ActiveCancelButton = true;
                Cancel();
            }
            else return;
        }

        protected void OnSelectedPositionChanged()
        {
            if (!ShowDiagram) ShowDiagram = false;

            if (!ShowTabl) ShowTabl = false;

            if (SelectedPosition == null) return;

            if (SelectedPosition.DocPos_StatKey != 55 && SelectedPosition.DocPos_StatKey != 125 /*&& SelectedPosition.DocPos_StatKey == 60*/)
            {
                ActiveCancelButton = false;
            }
            else ActiveCancelButton = true;

            if (SelectedPosition.DocPos_StatKey == 55)
            {
                ActiveApproveButton = false;
            }
            else if (SelectedPosition.DocPos_StatKey == 125)
                ActiveApproveButton = false;
            else if (SelectedPosition.DocPos_StatKey == 130)
                ActiveApproveButton = false;
            else if (SelectedPosition.DocPos_StatKey == 60)
            {
                ActiveApproveButton = false;
                ActiveCancelButton = false;
            }

            else ActiveApproveButton = true;


            int docPosId = SelectedPosition.DocPos_Id;
            int itemId = SelectedPosition.Item_Id;

            var barlist = _provData.GetReqFactStock(docPosId, itemId);

            var reqDate = DateTime.Today;

            var reqQty = 0.0;

            var stockQty = barlist.First(t => t.Type == BarType.Stock).Qty;

            var factQty = barlist.First(t => t.Type == BarType.Fact).Qty;
            var factDate = barlist.First(t => t.Type == BarType.Fact).Date;


            if (barlist.Count >= 3)
            {
                reqDate = barlist.First(t => t.Type == BarType.Req).Date;
                reqQty = barlist.First(t => t.Type == BarType.Req).Qty;
            }

            var reqList = _provData.GetProvOrd(SelectedPosition.DocPos_Id, reqDate);

            var diagramData = new ReqDataDiagram((int)reqQty, reqDate,
                (int)stockQty, (int)factQty, factDate, reqList);

            ReqOverview = reqList;

            DiagramData = diagramData;
        }

        public void GoToOrder()
        {
            if (SelectedPosition == null) return;

            var orderToGo = _dataAccess.OrderData.Get(SelectedPosition.Doc_Id, true);

            IoC.Instance.GetInstance<OrderEditVM>().Setup(orderToGo.Id);
            AllowEdit = false;

            _moduleVM.Navigate("OrderEditView", "Анкета заказа");
        }

        public void UpdateFields(DocumentPosVM posDoc, dynamic SelectedPosition)
        {
            posDoc.Art = SelectedPosition.Item_Art;
            posDoc.Id = SelectedPosition.DocPos_Id;
            posDoc.ArtId = SelectedPosition.Item_Id;
            posDoc.PosNr = SelectedPosition.DocPos_PosNr;
            posDoc.DocId = SelectedPosition.Doc_Id;
            posDoc.ParentDocId = SelectedPosition.DocContr_Id;
            posDoc.Qty = SelectedPosition.DocPos_Qty;
            posDoc.Price = SelectedPosition.DocPos_Price;
            posDoc.PricePos = SelectedPosition.DocPos_PricePos;
            posDoc.PricePosWithDiscontReb = SelectedPosition.DocPos_PricePosWithDiscontReb;
            posDoc.PricePosWithNds = SelectedPosition.DocPos_PricePosWithNds;
            posDoc.Nds = SelectedPosition.DocPos_Nds;
            posDoc.DiscontReb = SelectedPosition.DocPos_Discontreb;
            posDoc.PlanDate = SelectedPosition.DocPos_Plandate;
            // posDoc.FactDate = SelectedPosition.DocPos_Factdate;
            posDoc.ShipDate = SelectedPosition.DocPos_Shipdate;
            posDoc.PosStatus = SelectedPosition.DocPos_StatKey; 
            posDoc.ShipQty = SelectedPosition.DocPos_ShipQty;
            posDoc.ContractDate = SelectedPosition.DocPos_Contractdate;
            // posDoc.Dvers = SelectedPosition.DocPos_Dvers;

        }
    }
}
