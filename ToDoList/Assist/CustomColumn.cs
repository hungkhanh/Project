﻿using Syncfusion.UI.Xaml.Kanban;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

namespace ToDoList.Assist
{
    public class CustomColumn : KanbanColumn
    {

        #region Fields

        private ControlTemplate collapsedTemplate;
        private ControlTemplate expandedTemplate;

        #endregion

        #region ctor
        public CustomColumn()
        {
            PointerReleased += KanbanColumnAdv_PointerReleased;
            expandedTemplate = KanbanDictionaries.GenericCommonDictionary["ExpandedTemplate"] as ControlTemplate;
            collapsedTemplate = KanbanDictionaries.GenericCommonDictionary["CollapsedTemplate"] as ControlTemplate;
        }

        #endregion


        #region Properties

        public new bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public new static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            "IsExpanded", typeof(bool), typeof(CustomColumn), new PropertyMetadata(true, OnIsExpandedChanged));

        public ControlTemplate CollapsedColumnTemplate
        {
            get { return (ControlTemplate)GetValue(CollapsedColumnTemplateProperty); }
            set { SetValue(CollapsedColumnTemplateProperty, value); }
        }

        
        public static readonly DependencyProperty CollapsedColumnTemplateProperty =
            DependencyProperty.Register("CollapsedColumnTemplate", typeof(ControlTemplate), typeof(CustomColumn), new PropertyMetadata(null));

        #endregion


        #region Methods

        private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomColumn column = d as CustomColumn;

            column.ExpandedChanged((bool)e.NewValue);
        }

        private void ExpandedChanged(bool isExpanded)
        {
            if (Tags != null)
                Tags.IsExpanded = IsExpanded;

            if (!isExpanded)
            {
                ClearValue(Control.WidthProperty);
                Width = 50;

                ClearValue(Control.TemplateProperty);

                if (CollapsedColumnTemplate != null)
                {
                    Template = CollapsedColumnTemplate;
                }
                else if (collapsedTemplate != null)
                {
                    Template = collapsedTemplate;
                }

                foreach (KanbanCardItem card in Cards)
                {
                    card.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                ClearValue(Control.WidthProperty);

                PropertyInfo propInfo = typeof(KanbanColumn).GetProperty("Area", BindingFlags.Instance |
                                                                                 BindingFlags.NonPublic |
                                                                                 BindingFlags.Public);

                SfKanban area = propInfo.GetValue(this) as SfKanban;

                if (area != null)
                {
                    Binding binding = new Binding()
                    {
                        Source = area,
                        Path = new PropertyPath("ActualColumnWidth")
                    };
                    SetBinding(Control.WidthProperty, binding);
                }

                if (expandedTemplate != null)
                {
                    Template = expandedTemplate;
                }

                foreach (KanbanCardItem card in Cards)
                {
                    card.Visibility = Visibility.Visible;
                }
            }
        }

        private void KanbanColumnAdv_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.OriginalSource is Border &&
                    ((e.OriginalSource as Border).Name == "CollapsedIcon"))
            {
                IsExpanded = false;
            }
            else if (!IsExpanded)
            {
                IsExpanded = true;
            }
        }

        #endregion
    }
}
