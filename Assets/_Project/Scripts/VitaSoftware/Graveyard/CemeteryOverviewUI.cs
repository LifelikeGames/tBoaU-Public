using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;
using VitaSoftware.Logistics;
using VitaSoftware.Shop;
using VitaSoftware.Underworld;

namespace VitaSoftware.Graveyard
{
    //Max size is 7x15
    public class CemeteryOverviewUI : MonoBehaviour
    {
        [SerializeField] private PlotUI plotPrefab;
        [SerializeField] private GameObject cemeteryOverviewDisplay;
        [SerializeField] private Transform plotParent;
        [SerializeField] private ShopManager shopManager;
        [SerializeField] private int width =7, height = 2;
        [SerializeField] private TextMeshProUGUI currentOrderText;
        [SerializeField] private GridLayoutGroup grid;
        [SerializeField] private GameObject sellCorpseWindow;
        [SerializeField] private UnderworldManager underworldManager;
        [SerializeField] private GraveyardManager graveyardManager;
        [SerializeField] private StockZone stockZone;

        private Order currentOrder;
        private List<PlotUI> plots;

        private void Awake()
        {
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = width;
            plots = new();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var plot = Instantiate(plotPrefab, plotParent);
                    plots.Add(plot);
                    var button = plot.GetComponent<Button>();
                    button.onClick.AddListener(()=>ClickOnPlot(plots.IndexOf(plot)));
                    plot.SetLabel(GetCoordinateString(y,x));
                    
                }
            }
            
            sellCorpseWindow.SetActive(false);
            cemeteryOverviewDisplay.SetActive(false);
        }

        private string GetCoordinateString(int i, int j)
        {
            return ((char) (65+j)).ToString()+i;
        }

        //TODO: open UI
        //TODO: get list of delivered orders (that contain the gravestones, decorations, etc...)
        //TODO: phase 1, select plot for each order, gravestone and other stuff are placed immediately
        //TODO: phase 2, as before except plot is placed after arrival of hearse and burial party
        //TODO: phase 3, add mourning room, split apart placement of gravestone and arrival of coffin
        public void PlaceOrders()
        {
            cemeteryOverviewDisplay.SetActive(true);
            DisplayNextOrder();
        }

        private void DisplayNextOrder()
        {
            currentOrder = shopManager.OrdersToPlace.FirstOrDefault();
            if (currentOrder == null)
            {
                Debug.LogError("Failed to find any orders to place");
                return;
            }

            currentOrderText.text = "Next order: Order " + currentOrder.id + " with " + currentOrder.gravestone.Label;
        }

        private int currentCorpseIndex;
        public void ClickOnPlot(int index)
        {
            if (plots[index].IsOccupied)
            {
                if (!underworldManager.IsActive) return;
                
                currentCorpseIndex = index;
                OpenSellCorpseWindow();

                return;
            } 
            if(shopManager.OrdersToPlace.Count < 1 || currentOrder == null) return;
            plots[index].ItemsPlaced();
            shopManager.OrdersToPlace.Remove(currentOrder);
            shopManager.PlaceGravestone(currentOrder.gravestone, index);
            shopManager.PlaceGraveDecorations();
            //stockZone.UpdateCrates(shopManager.OrdersToPlace);

            if (shopManager.OrdersToPlace.Count > 0)
                DisplayNextOrder();
            else
                currentOrderText.text = "No orders left to place";
        }

        private void OpenSellCorpseWindow()
        {
            sellCorpseWindow.SetActive(true);
        }

        public void SellCorpse()
        {
            underworldManager.SellCorpse();
            plots[currentCorpseIndex].CorpseRemoved();
            graveyardManager.EmptySpot(currentCorpseIndex);
            
            sellCorpseWindow.SetActive(false);
        }

        public void CancelSellCorpse()
        {
            sellCorpseWindow.SetActive(false);
        }
    }
}