using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace UnityEditor.Recorder.Input
{
    [DisplayName("Audio")]
    [Serializable]
    public class AudioInputSettings : RecorderInputSettings
    {
        public bool preserveAudio = true;

        protected internal override Type inputType
        {
            get { return typeof(AudioInput); }
        }

        protected internal override bool ValidityCheck(List<string> errors)
        {
            return true;
        }
    }
}
