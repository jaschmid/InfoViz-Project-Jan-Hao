using System;
using System.Collections.Generic;
using System.Text;

using Gav.Graphics;
using Gav.Data;

using Microsoft.DirectX;
using System.Drawing;
using Gav.Components.Internal;
using Gav.Components.GavLayers;
using Gav.Management;
using Microsoft.DirectX.Direct3D;
using System.Windows.Forms;

namespace InfoVizProject
{
    public partial class CustomComponent
    {

        class InteractionLayer : Layer
        {

            private ContextMenu rClickMenu = new ContextMenu();
            private MenuItem xAxisSource = new MenuItem("X Axis Source");
            private MenuItem yAxisSource = new MenuItem("Y Axis Source");
            private MenuItem thicknessSource = new MenuItem("Line Thickness Source");
            private MenuItem findClosest = new MenuItem("Select closest Lines");

            public CustomComponent parent
            {
                get;
                set;
            }

            public bool AllowCustomizeXAxis { get; set; }
            public bool AllowCustomizeYAxis { get; set; }
            public bool AllowCustomizeThickness { get; set; }


            public event SelectionUpdatedEventHandler SelectionChanged;

            private List<int> selectedItems;

            private LineLayer lineLayer;

            public List<int> SelectedItems
            {
                get
                {
                    return selectedItems;
                }
                set
                {
                    selectedItems = value;
                }
            }


            public InteractionLayer(LineLayer lineLayer,CustomComponent parent)
            {
                this.lineLayer = lineLayer;
                this.parent = parent;
                this.findClosest.Click += delegate(object o, EventArgs e)
                {
                    this.selectedFindCloseLine();
                };
            }

            protected void Initialize(Device device)
            {
                _inited = true;
            }

            protected override void Render(Device device)
            {
                if (!_inited) Initialize(device);

            }

            private delegate void HandleSelectionChanged(int value);

            private void PopulateAxisSource(MenuItem item,int selected, HandleSelectionChanged handler)
            {
                item.MenuItems.Clear();
                MenuItem m = new MenuItem("None");
                m.Click += delegate(object o, EventArgs e)
                {
                    handler(-1);
                };
                m.Index = -1;
                if (selected == -1)
                    m.Checked = true;
                item.MenuItems.Add(m);
                if (this.parent.DataLabels != null)
                {
                    for (int i = 0; i < this.parent.DataLabels.Count; ++i)
                    {

                        m = new MenuItem(this.parent.DataLabels[i]);
                        m.Index = i;
                        int local = i;
                        m.Click += delegate(object o, EventArgs e)
                        {
                            handler(local);
                        };
                        if (i == selected)
                            m.Checked = true;
                        item.MenuItems.Add(m);
                    }
                }
            }

            protected override bool MouseDown(Gav.Components.Events.LayerMouseButtonEventArgs e)
            {/*
                Vector2 pos = e.GetLayerRelativePosition();
                if (_boxPosition.X + _scale > pos.X && _boxPosition.X - _scale < pos.X &&
                    _boxPosition.Y + _scale > pos.Y && _boxPosition.Y - _scale < pos.Y)
                {
                    _moveBox = true;
                    _mouseDownDelta.X = _boxPosition.X - pos.X;
                    _mouseDownDelta.Y = _boxPosition.Y - pos.Y;
                    // Returnning true locks the mouse to this layer until false is returned
                    return true;
                */
                if (e.MouseButton == System.Windows.Forms.MouseButtons.Left)
                {
                    this.lastMousePosition = e.Location;
                    this.movementLag = new Vector2(0.0f, 0.0f);
                    this.translating = false;
                    this.mousedown[0] = true;
                }
                else if (e.MouseButton == System.Windows.Forms.MouseButtons.Right)
                {
                    this.lastMousePosition = e.Location;
                    this.movementLag = new Vector2(0.0f, 0.0f);
                    this.scaling = false;
                    this.mousedown[1] = true;
                }
                /*
                    
                }*/

                return true;
            }

            private Point lastMousePosition;
            private Vector2 movementLag;
            private bool translating;
            private bool scaling;
            private bool[] mousedown = new bool[2];

            protected override bool MouseMove(Gav.Components.Events.LayerMouseMoveEventArgs e)
            {
                if (e.MouseButton.HasFlag(System.Windows.Forms.MouseButtons.Left) && this.mousedown[0])
                {
                    Vector2 relative = (new Vector2(e.Location.X - this.lastMousePosition.X, this.lastMousePosition.Y - e.Location.Y));

                    if (this.translating)
                    {
                        lineLayer.Translation += relative;
                        parent.Invalidate();
                    }
                    else
                    {
                        this.movementLag += relative;
                        if (this.movementLag.LengthSq() > 3.0f)
                        {
                            lineLayer.Translation += this.movementLag;
                            this.translating = true;
                            parent.Invalidate();
                        }
                    }
                }
                else if (e.MouseButton.HasFlag(System.Windows.Forms.MouseButtons.Right) && this.mousedown[1])
                {
                    Vector2 relative = (new Vector2(e.Location.X - this.lastMousePosition.X, this.lastMousePosition.Y - e.Location.Y));

                    if (this.scaling)
                    {
                        this.lineLayer.Zoom += (float)relative.Y / 100.0f;
                        this.lineLayer.Translation = new Vector2(
                                this.lineLayer.Translation.X - this.lastMousePosition.X * (float)relative.Y / 100.0f,
                                this.lineLayer.Translation.Y - (this.Control.AbsoluteSize.Height - this.lastMousePosition.Y) * (float)relative.Y / 100.0f);
                        parent.Invalidate();
                    }
                    else
                    {
                        this.movementLag += relative;
                        if (this.movementLag.LengthSq() > 3.0f)
                        {
                            Vector2 oldTrans = this.lineLayer.Translation * (1.0f/this.lineLayer.Zoom);
                            this.lineLayer.Zoom += (float)this.movementLag.Y / 100.0f;
                            oldTrans *= this.lineLayer.Zoom;
                            this.lineLayer.Translation = new Vector2(
                                    oldTrans.X - this.lastMousePosition.X * (float)this.movementLag.Y / 100.0f,
                                    oldTrans.Y - (this.Control.AbsoluteSize.Height - this.lastMousePosition.Y) * (float)this.movementLag.Y / 100.0f);
                            this.scaling = true;
                            parent.Invalidate();
                        }
                    }
                }
                this.lastMousePosition = e.Location;
                return true;
            }

            protected override bool MouseUp(Gav.Components.Events.LayerMouseButtonEventArgs e)
            {/*
                _moveBox = false;*/

                if (e.MouseButton == System.Windows.Forms.MouseButtons.Left)
                {
                    this.mousedown[0] = false;
                    if (!this.translating)
                    {
                        List<int> selected = new List<int>();
                        selected.Add(this.lineLayer.GetClosestLine(e.Location.X, e.Location.Y));
                        this.parent.SetSelectedIndexes(selected);
                        SelectionUpdatedEventArgs eventArgs = new SelectionUpdatedEventArgs();
                        eventArgs.SelectedItems = selected;
                        this.SelectionChanged(this, eventArgs);
                    }
                }
                else if (e.MouseButton == System.Windows.Forms.MouseButtons.Right)
                {
                    this.mousedown[1] = false;
                    if (!this.scaling)
                    {
                        this.rClickMenu.MenuItems.Clear();
                        if (this.AllowCustomizeXAxis)
                        {
                            this.PopulateAxisSource(this.xAxisSource, this.parent.DataLineXIndex, this.selectedXIndex);
                            this.rClickMenu.MenuItems.Add(this.xAxisSource);
                        }
                        if (this.AllowCustomizeYAxis)
                        {
                            this.PopulateAxisSource(this.yAxisSource, this.parent.DataLineYIndex, this.selectedYIndex);
                            this.rClickMenu.MenuItems.Add(this.yAxisSource);
                        }
                        if (this.AllowCustomizeThickness)
                        {
                            this.PopulateAxisSource(this.thicknessSource, this.parent.DataLineThicknessIndex, this.selectedThicknessIndex);
                            this.rClickMenu.MenuItems.Add(this.thicknessSource);
                        }
                        if (this.selectedItems == null || this.selectedItems.Count == 0)
                            this.findClosest.Enabled = false;
                        else
                            this.findClosest.Enabled = true;
                        this.rClickMenu.MenuItems.Add(this.findClosest);

                        if (this.parent.UserMenu != null)
                        {
                            this.rClickMenu.MenuItems.Add("-");

                            foreach (MenuItem i in this.parent.UserMenu.MenuItems)
                                if (i != null)
                                {
                                    this.rClickMenu.MenuItems.Add(i.CloneMenu());
                                }
                        }


                        rClickMenu.Show(this.parent.iRenderTarget, e.Location);
                    }
                }
                return true;
            }

            protected override bool MouseWheel(Gav.Components.Events.LayerMouseWheelEventArgs e)
            {
               /* 
                parent.Invalidate();*/

                /*
                _scale += e.WheelDelta / 1000.0f * Math.Abs(_scale);
                Render();
                // Return false to continue sending MouseWheel to the other layers,
                // return true to stop sending the event.
                */
                return true;
            }

            private void selectedXIndex(int newValue)
            {
                parent.DataLineXIndex = newValue;
                parent.Invalidate();
            }

            private void selectedYIndex(int newValue)
            {
                parent.DataLineYIndex = newValue;
                parent.Invalidate();
            }

            private void selectedThicknessIndex(int newValue)
            {
                parent.DataLineThicknessIndex = newValue;
                parent.Invalidate();
            }

            private void selectedFindCloseLine()
            {
                int old = this.selectedItems[0];
                List<int> proposed = lineLayer.GetClosestLinesToLine(old);
                List<int> selected = new List<int>();
                if (proposed.Count > 0)
                {
                    selected.Add(proposed[0]);
                    selected.Add(old);
                }
                this.SelectedItems = selected;
                this.parent.SetSelectedIndexes(selected);
                SelectionUpdatedEventArgs eventArgs = new SelectionUpdatedEventArgs();
                eventArgs.SelectedItems = selected;
                this.SelectionChanged(this, eventArgs);
            }

        }
    }
}