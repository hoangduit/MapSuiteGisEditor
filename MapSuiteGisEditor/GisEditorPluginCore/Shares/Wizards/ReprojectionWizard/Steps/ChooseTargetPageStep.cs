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
using System.Linq;

namespace ThinkGeo.MapSuite.GisEditor.Plugins
{
    [Serializable]
    internal class ChooseTargetPageStep : WizardStep<ReprojectionShareObject>
    {
        private ReprojectionShareObject model;

        public ChooseTargetPageStep()
        {
            Title = GisEditor.LanguageManager.GetStringResource("GeneralStepOne");
            Header = GisEditor.LanguageManager.GetStringResource("ChooseTargetPageStepHeader"); 
            Description = GisEditor.LanguageManager.GetStringResource("ChooseTargetPageStepHeader"); 
        }

        protected override void EnterCore(ReprojectionShareObject parameter)
        {
            model = parameter;
            Content = new ChooseTargetPage(parameter);
            base.EnterCore(parameter);
        }

        protected override bool CanMoveToNextCore()
        {
            return model.SourceFiles.All(s => s.IsInternalProjectionDetermined) && model.IsExternalProjectionDetermined;
        }
    }
}