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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ThinkGeo.MapSuite.Layers;

namespace ThinkGeo.MapSuite.GisEditor.Plugins
{
    [Serializable]
    public class DissolveWizardChooseLayerViewModel : INotifyPropertyChanged
    {
        private bool hasSelectedFeatures;
        private bool dissolveSelectedFeaturesOnly;
        private FeatureLayer selectedFeatureLayer;
        private ObservableCollection<FeatureLayer> featureLayers;

        public event PropertyChangedEventHandler PropertyChanged;

        public DissolveWizardChooseLayerViewModel()
        {
            featureLayers = new ObservableCollection<FeatureLayer>();
        }

        public ObservableCollection<FeatureLayer> FeatureLayers
        {
            get { return featureLayers; }
        }

        public FeatureLayer SelectedFeatureLayer
        {
            get { return selectedFeatureLayer; }
            set
            {
                selectedFeatureLayer = value;
                HasFeatureSelected = GisEditor.SelectionManager.GetSelectedFeatures().Any(f => f.Tag == SelectedFeatureLayer);
                OnPropertyChanged("SelectedFeatureLayer");
            }
        }

        public bool DissolveSelectedFeaturesOnly
        {
            get { return dissolveSelectedFeaturesOnly; }
            set
            {
                dissolveSelectedFeaturesOnly = value;
                OnPropertyChanged("DissolveSelectedFeaturesOnly");
            }
        }

        public bool HasFeatureSelected
        {
            get
            {
                return hasSelectedFeatures;
            }
            set
            {
                hasSelectedFeatures = value;
                OnPropertyChanged("HasFeatureSelected");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
