using System;

namespace MaiksMixer.Core.Models
{
    /// <summary>
    /// Represents information about a configuration preset.
    /// </summary>
    public class PresetInfo
    {
        /// <summary>
        /// Gets or sets the unique identifier for the preset.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the preset.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the preset.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the category of the preset.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the preset was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the preset was last modified.
        /// </summary>
        public DateTime ModifiedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the preset is marked as a favorite.
        /// </summary>
        public bool IsFavorite { get; set; }

        /// <summary>
        /// Gets or sets the path to the preset file.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the settings data associated with this preset.
        /// </summary>
        public ApplicationSettings Settings { get; set; }

        /// <summary>
        /// Initializes a new instance of the PresetInfo class.
        /// </summary>
        public PresetInfo()
        {
            Id = Guid.NewGuid().ToString();
            Name = "New Preset";
            Description = "";
            Category = "Default";
            CreatedAt = DateTime.Now;
            ModifiedAt = DateTime.Now;
            IsFavorite = false;
        }

        /// <summary>
        /// Creates a deep copy of this preset info.
        /// </summary>
        /// <returns>A new PresetInfo instance with the same values.</returns>
        public PresetInfo Clone()
        {
            return new PresetInfo
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Category = this.Category,
                CreatedAt = this.CreatedAt,
                ModifiedAt = this.ModifiedAt,
                IsFavorite = this.IsFavorite,
                FilePath = this.FilePath,
                Settings = this.Settings?.Clone()
            };
        }
    }
}
