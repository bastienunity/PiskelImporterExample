using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Formats.Piskel;
using UnityEditor;
using UnityEngine;
using UnityEditor.Recorder;

namespace Unity.PiskelRecorder
{
    class PiskelRecorder : BaseTextureRecorder<PiskelRecorderSettings>
    {
        List<Texture2D> _renderedFrames;
        int _currentFrame;
        bool _recordingComplete;

        protected override TextureFormat readbackTextureFormat
        {
            get { return TextureFormat.RGBA32; }
        }

        public override void EndRecording(RecordingSession session)
        {
            _recordingComplete = true;
        }

        public override bool BeginRecording(RecordingSession session)
        {
            if (!base.BeginRecording(session))
            {
                return false;
            }

            _recordingComplete = false;
            _renderedFrames = new List<Texture2D>();
            _currentFrame = 0;
            m_Settings.FileNameGenerator.CreateDirectory(session);

            return true;
        }

        public override void RecordFrame(RecordingSession session)
        {
            if (m_Inputs.Count != 1)
                throw new Exception("Unsupported number of sources");

            _currentFrame++;
            base.RecordFrame(session);
        }

        protected override void WriteFrame(Texture2D tex)
        {
            var copyTexture = new Texture2D(tex.width, tex.height);
            copyTexture.SetPixelData(tex.GetRawTextureData(), 0);
            _renderedFrames.Add(copyTexture);

            if (_renderedFrames.Count == _currentFrame && _recordingComplete)
            {
                WritePiskelOutput();
            }
        }

        private void WritePiskelOutput()
        {
            var outputWidth = m_Settings.imageInputSettings.outputWidth;
            var outputHeight = m_Settings.imageInputSettings.outputHeight;

            var finalWidth = outputWidth * _renderedFrames.Count();
            if (finalWidth > SystemInfo.maxTextureSize)
            {
                Debug.Log("The output texture width is too large, try lowering the capture resolution or make the animation shorter");
                return;
            }

            Texture2D renderedFrame = new Texture2D(outputWidth * _currentFrame, outputHeight);
            for (var index = 0; index < _renderedFrames.Count; index++)
            {
                var pixels = _renderedFrames[index].GetPixels32();
                renderedFrame.SetPixels32(index * outputWidth, 0, outputWidth, outputHeight, pixels);
            }
            _renderedFrames.Clear();

            PiskelChunk[] chunks = new PiskelChunk[1];
            chunks[0] = new PiskelChunk();
            chunks[0].SetTexture(renderedFrame);
            chunks[0].layout = new PiskelChunk.IntArray[_currentFrame];
            for (int i = 0; i < _currentFrame; i++)
            {
                chunks[0].layout[i] = new PiskelChunk.IntArray() {array = new[] {i}};
            }

            PiskelDocument piskelDocument = new PiskelDocument
            {
                name = Path.GetFileNameWithoutExtension(settings.outputFile),
                width = outputWidth,
                height = outputHeight,
                fps = m_Settings.outputFrameRate,
                layers = new PiskelLayer[1]
            };
            piskelDocument.layers[0] = new PiskelLayer
            {
                chunks = chunks,
                name = "Layer0",
                frameCount = _currentFrame,
                opacity = 1.0f
            };

            var jsonString = piskelDocument.ToPiskelJson();
            File.WriteAllText(settings.outputFile, jsonString);
            AssetDatabase.Refresh();
        }
    }
}
