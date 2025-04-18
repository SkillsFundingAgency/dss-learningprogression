﻿using DFC.Swagger.Standard.Annotations;
using NCS.DSS.LearningProgression.ReferenceData;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NCS.DSS.LearningProgression.Models
{
    public class LearningProgressionPatch : ILearningProgression
    {
        [Display(Description = "Unique identifier for a learning progression record.")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        [JsonPropertyName("id")]
        public Guid? LearningProgressionId { get; set; }

        [Display(Description = "Unique identifier of a customer.")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        public Guid? CustomerId { get; set; }

        [DataType(DataType.DateTime)]
        [Example(Description = "2018-06-21T17:45:00")]
        [Display(Description = "Date and time date progression recorded.")]
        public DateTime? DateProgressionRecorded { get; set; }

        [Display(Description = "Learning progression Learning Status.")]
        [Example(Description = "3")]
        public CurrentLearningStatus? CurrentLearningStatus { get; set; }

        [Display(Description = "Learning progression Learning Hours.")]
        [Example(Description = "2")]
        public LearningHours? LearningHours { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time learning started.")]
        [Example(Description = "2018-06-21T17:45:00")]
        public DateTime? DateLearningStarted { get; set; }

        [Display(Description = "Learning progression Qualification Level.")]
        [Example(Description = "3")]
        public QualificationLevel? CurrentQualificationLevel { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time Qualification level achieved.")]
        [Example(Description = "2018-06-21T17:45:00")]
        public DateTime? DateQualificationLevelAchieved { get; set; }

        [StringLength(8, MinimumLength = 8)]
        public string? LastLearningProvidersUKPRN { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of last modification.")]
        [Example(Description = "2018-06-21T17:45:00")]
        public DateTime? LastModifiedDate { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "0000000001")]
        public string? LastModifiedTouchpointId { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [JsonIgnore]
        public string? CreatedBy { get; set; }
    }
}
