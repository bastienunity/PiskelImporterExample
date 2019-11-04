using System.Collections.Generic;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace Unity.PiskelRecorder
{
    [RecorderSettings(typeof(PiskelRecorder), "Piskel", "imagesequence_16")]
    public class PiskelRecorderSettings : RecorderSettings
    {
        public bool captureAlpha;
        public int outputFrameRate = 5;
        [SerializeField] public GameViewInputSettings gameViewInputSettings = new GameViewInputSettings();

        public PiskelRecorderSettings()
        {
            FileNameGenerator.fileName = "animation.piskel";
            gameViewInputSettings.outputWidth = 256;
            gameViewInputSettings.outputHeight = 256;
        }

        public ImageInputSettings imageInputSettings
        {
            get { return gameViewInputSettings; }
        }

        public override string extension
        {
            get { return "piskel"; }
        }

        protected override bool ValidityCheck(List<string> errors)
        {
            var ok = base.ValidityCheck(errors);

            if (string.IsNullOrEmpty(FileNameGenerator.fileName))
            {
                ok = false;
                errors.Add("missing file name");
            }

            if (outputFrameRate <= 0)
                outputFrameRate = 1;

            return ok;
        }

        public override IEnumerable<RecorderInputSettings> inputsSettings
        {
            get { yield return gameViewInputSettings; }
        }

        public override void SelfAdjustSettings()
        {
            gameViewInputSettings.flipFinalOutput = SystemInfo.supportsAsyncGPUReadback;
        }
    }
}
