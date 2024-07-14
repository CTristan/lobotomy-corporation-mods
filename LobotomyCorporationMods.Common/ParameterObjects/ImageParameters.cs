// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.ParameterObjects
{
    /// <summary>Represents the parameters of an image.</summary>
    public sealed class ImageParameters
    {
        /// <summary>A specific ID for the image to reference later.</summary>
        public string ImageId { get; set; }

        /// <summary>The full path of the image.</summary>
        public string ImageFilePath { get; set; }

        /// <summary>The X position of the image.</summary>
        public float LocalPositionX { get; set; }

        /// <summary>The Y local position of the image.</summary>
        public float LocalPositionY { get; set; }

        /// <summary>The Z position of the image.</summary>
        public float LocalPositionZ { get; set; }

        /// <summary>The X scale of the image.</summary>
        public float LocalScaleX { get; set; }

        /// <summary>The Y scale of the image.</summary>
        public float LocalScaleY { get; set; }
    }
}
