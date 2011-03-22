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

namespace InfoVizProject
{
    public partial class CustomComponent
    {

        class InteractionLayer : Layer
        {

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


            public InteractionLayer(LineLayer lineLayer)
            {
                this.lineLayer = lineLayer;

            }

            protected void Initialize(Device device)
            {
                _inited = true;
            }

            protected override void Render(Device device)
            {
                if (!_inited) Initialize(device);

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
                selectedItems = new List<int>();
                selectedItems.Add(lineLayer.GetClosestLine(e.Location.X,e.Location.Y));
                SelectionUpdatedEventArgs eventArgs = new SelectionUpdatedEventArgs();
                eventArgs.SelectedItems = this.selectedItems;
                lineLayer.SetSelectedIndexes(this.selectedItems);
                SelectionChanged(this, eventArgs);

                return true;
            }

            protected override bool MouseMove(Gav.Components.Events.LayerMouseMoveEventArgs e)
            {/*
                if (_moveBox)
                {
                    Vector2 pos = e.GetLayerRelativePosition();
                    _boxPosition = new Vector3(pos.X + _mouseDownDelta.X, pos.Y + _mouseDownDelta.Y, 0);
                    Render();
                    return true;
                }*/
                return false;
            }

            protected override bool MouseUp(Gav.Components.Events.LayerMouseButtonEventArgs e)
            {/*
                _moveBox = false;*/
                return false;
            }

            protected override bool MouseWheel(Gav.Components.Events.LayerMouseWheelEventArgs e)
            {/*
                _scale += e.WheelDelta / 1000.0f * Math.Abs(_scale);
                Render();
                // Return false to continue sending MouseWheel to the other layers,
                // return true to stop sending the event.
                */
                return true;
            }

        }
    }
}