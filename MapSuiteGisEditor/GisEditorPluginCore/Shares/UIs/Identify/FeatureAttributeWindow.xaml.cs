/*
* Licensed to the Apache Software Foundation (ASF) under one
* or more contributor license agreements.  See the NOTICE file
* distributed with this work for additional information
* regarding copyright ownership.  The ASF licenses this file
* to you under the Apache License, Version 2.0 (the
* "License"); you may not use this file except in compliance
* with the License.  You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ThinkGeo.MapSuite.Layers;
using ThinkGeo.MapSuite.Shapes;
using ThinkGeo.MapSuite.WpfDesktop.Extension;

namespace ThinkGeo.MapSuite.GisEditor.Plugins
{
    /// <summary>
    /// Interaction logic for FeatureInfoUserControl.xaml
    /// </summary>
    public partial class FeatureAttributeWindow : Window
    {
        public event EventHandler<RoutedPropertyChangedEventArgs<object>> SelectionFeatureChanged;

        private string originalColumnValue;
        private Action<DataGridCellEditEndingEventArgs> dataGridCellEditEndingDelegate;

        public FeatureAttributeWindow(Dictionary<FeatureLayer, Collection<Feature>> features, bool alwaysPrompt)
        {
            InitializeComponent();
            Closing += FeatureAttributeWindow_Closing;
            chbPrompt.IsChecked = alwaysPrompt;

            Collection<FeatureLayerViewModel> layerEntities = new Collection<FeatureLayerViewModel>();
            foreach (var item in features)
            {
                FeatureLayerViewModel featureLayerModel = new FeatureLayerViewModel();
                featureLayerModel.LayerName = item.Key.Name;
                foreach (var feature in item.Value)
                {
                    featureLayerModel.FoundFeatures.Add(new FeatureViewModel(feature, item.Key));
                }
                layerEntities.Add(featureLayerModel);
            }

            featuresList.ItemsSource = layerEntities;

            if (layerEntities.Count > 0)
            {
                layerEntities[0].FoundFeatures.FirstOrDefault().IsSelected = true;
            }

            DataGridCellEditEndingDelegate = GridCellEditing;
        }

        public Action<DataGridCellEditEndingEventArgs> DataGridCellEditEndingDelegate
        {
            get { return dataGridCellEditEndingDelegate; }
            set { dataGridCellEditEndingDelegate = value; }
        }

        public Visibility AlwaysPromptVisibility
        {
            get { return chbPrompt.Visibility; }
            set { chbPrompt.Visibility = value; }
        }

        public Func<bool> CancelDelegate { get; set; }

        public FeatureViewModel SelectedItem
        {
            get { return featuresList.SelectedItem as FeatureViewModel; }
        }

        protected virtual void OnSelectionFeatureChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            EventHandler<RoutedPropertyChangedEventArgs<object>> handler = SelectionFeatureChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [Obfuscation]
        private void featuresList_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            FeatureViewModel entity = featuresList.SelectedValue as FeatureViewModel;
            if (entity != null)
            {
                e.Handled = true;
            }
            else
            {
                FeatureLayerViewModel layerEntity = featuresList.SelectedValue as FeatureLayerViewModel;
                if (layerEntity != null)
                {
                    layerEntity.FoundFeatures.FirstOrDefault().IsSelected = true;
                }
            }
            OnSelectionFeatureChanged(e);
        }

        [Obfuscation]
        private void featureNode_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement clickedElement = sender as FrameworkElement;
            if (clickedElement != null)
            {
                FeatureViewModel entity = clickedElement.DataContext as FeatureViewModel;
                if (entity != null)
                {
                    GisEditor.ActiveMap.CurrentExtent = entity.Feature.GetBoundingBox();
                    GisEditor.ActiveMap.Refresh();
                }
            }
        }

        [Obfuscation]
        private void featureAttributeGrid_AutoGeneratedColumns(object sender, EventArgs e)
        {
            foreach (DataGridColumn column in featureAttributeGrid.Columns)
            {
                if (column.Header.ToString() == "Column")
                {
                    column.IsReadOnly = true;
                }
                else
                {
                    column.Width = 300;
                }
            }
        }

        [Obfuscation]
        private void featureAttributeGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            DataRowView row = e.Row.Item as DataRowView;
            originalColumnValue = row[e.Column.Header.ToString()].ToString();
        }

        [Obfuscation]
        private void featureAttributeGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (DataGridCellEditEndingDelegate != null)
            {
                DataGridCellEditEndingDelegate(e);
            }
        }

        private void GridCellEditing(DataGridCellEditEndingEventArgs e)
        {
            GisEditorEditInteractiveOverlay editOverlay = GisEditor.ActiveMap.FeatureLayerEditOverlay;
            InMemoryFeatureLayer layer = editOverlay != null ? editOverlay.EditShapesLayer : null;

            if (layer != null)
            {
                try
                {
                    FeatureViewModel entity = featuresList.SelectedValue as FeatureViewModel;
                    string columnName = ((DataRowView)e.Row.Item)[0].ToString();
                    string columnValue = (e.EditingElement as TextBox).Text;

                    if (columnValue != originalColumnValue)
                    {
                        layer.Open();
                        Feature feature = layer.QueryTools.GetFeatureById(entity.FeatureId, layer.GetDistinctColumnNames());
                        feature.ColumnValues[columnName] = columnValue;
                        if (!layer.EditTools.IsInTransaction) layer.EditTools.BeginTransaction();
                        layer.EditTools.Update(feature);
                        TransactionResult result = layer.EditTools.CommitTransaction();
                        if (result.TotalFailureCount != 0)
                        {
                            string failureReasonString = null;
                            Dictionary<string, string> failureReasons = result.FailureReasons;
                            foreach (string key in failureReasons.Keys)
                            {
                                failureReasonString += "\r\n\r\n" + failureReasons[key];
                            }
                            System.Windows.Forms.MessageBox.Show(GisEditor.LanguageManager.GetStringResource("FeatureAttibuteWindowUpdateFailedText") + failureReasonString, GisEditor.LanguageManager.GetStringResource("FeatureAttibuteWindowUpdateFailedCaption"));
                            (e.EditingElement as TextBox).Text = originalColumnValue;
                        }
                        else
                        {
                            // Update the selected value
                            entity.Feature.ColumnValues[columnName] = columnValue;
                        }
                    }
                }
                catch (UnauthorizedAccessException unauthorizedAccessException)
                {
                    GisEditor.LoggerManager.Log(LoggerLevel.Debug, unauthorizedAccessException.Message, new ExceptionInfo(unauthorizedAccessException));
                    System.Windows.Forms.MessageBox.Show(string.Format("{0} {1}", unauthorizedAccessException.Message, GisEditor.LanguageManager.GetStringResource("FeatureAttributeWindowRestartAdminText")), GisEditor.LanguageManager.GetStringResource("MessageBoxWarningTitle"), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    GisEditor.LoggerManager.Log(LoggerLevel.Debug, ex.Message, new ExceptionInfo(ex));
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
                finally
                {
                    layer.Close();
                }
            }
        }

        [Obfuscation]
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            bool isCanceled = true;
            if (CancelDelegate != null)
            {
                isCanceled = CancelDelegate();
            }

            if (isCanceled)
            {
                DialogResult = false;
            }
            else
            {
                DialogResult = true;
            }
            Close();
        }

        [Obfuscation]
        private void OKClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        [Obfuscation]
        private void featureAttributeGrid_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            var selector = sender as System.Windows.Controls.Primitives.MultiSelector;
            if (selector != null && textBox != null)
            {
                DataRowView dataRowView = (DataRowView)selector.SelectedItem;
                var columnName = dataRowView[0].ToString();
                if (dataRowView.Row[0] != null)
                {
                    if (columnName.Equals("TRACT", StringComparison.InvariantCultureIgnoreCase) || columnName.Equals("SUBTRACT", StringComparison.InvariantCultureIgnoreCase))
                    {
                        textBox.Text = textBox.Text.ToUpperInvariant();
                        textBox.SelectionStart = textBox.Text.Length;
                    }
                }
            }
        }

        [Obfuscation]
        private void FeatureInforGrid_OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.Equals("RealValue"))
            {
                e.Column.Visibility = Visibility.Collapsed;
            }
        }

        [Obfuscation]
        private void featureAttributeGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Width = 250;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Text = ((DataRowView)e.Row.Item).Row[1].ToString();
            if (!string.IsNullOrEmpty(textBlock.Text))
            {
                e.Row.ToolTip = textBlock;
            }
        }

        [Obfuscation]
        private void FeatureAttributeWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool value = chbPrompt.IsChecked.GetValueOrDefault();
            if (value != Singleton<EditorSetting>.Instance.IsAttributePrompted)
            {
                Singleton<EditorSetting>.Instance.IsAttributePrompted = value;
                var editPlugin = GisEditor.UIManager.GetPlugins().OfType<EditorUIPlugin>().FirstOrDefault();
                if (editPlugin != null) GisEditor.InfrastructureManager.SaveSettings(editPlugin);
            }
        }
    }
}