using DFC.Swagger.Standard.Annotations;
using Microsoft.Build.Framework;
using NCS.DSS.LearningProgression.Enumerations;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace NCS.DSS.LearningProgression.Models
{
    public class LearningProgression
    {
        [Display(Description = "Unique identifier for a learning progression record")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        [JsonProperty(PropertyName = "id")]
        public Guid LearningProgressionId { get; set; }

        [Display(Description = "Unique identifier of a customer")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        [Required]
        public Guid CustomerId { get; set; }

        [StringLength(50)]
        public string SubContractorId { get; set; }

        [Required]
        [Example(Description = "2018-06-21T17:45:00")]
        public DateTime DateProgressionRecorded { get; set; }

        [Required]
        [Display(Description = "Learning progression Learning Status:   " +
                                "1 - In learning,  " +
                                "2 - Not in learning,   " +
                                "3 - Traineeship,   " +
                                "4 - Prefer not to say,   " +
                                "99 - Not known")]
        [Example(Description = "3")]
        public CurrentLearningStatus CurrentLearningStatus { get; set; }

        [Display(Description = "Learning progression Learning Hours:   " +
                        "1 - Less than 16 hours,  " +
                        "2 - 16 hours or more,  ")]
        [Example(Description = "2")]
        public LearningHours LearningHours { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Example(Description = "2018-06-21T17:45:00")]
        public DateTime? DateLearningStarted { get; set; }

        [Required]
        [Display(Description = "Learning progression Qualification Level:   " +
                                "0 - Entry Level,  " +
                                "1 - Qualification Level 1,   " +
                                "2 - Qualification Level 2,   " +
                                "3 - Qualification Level 3,   " +
                                "4 - Qualification Level 4,  " +
                                "5 - Qualification Level 5,   " +
                                "6 - Qualification Level 6,   " +
                                "7 - Qualification Level 7,   " +
                                "8 - Qualification Level 8,   " +
                                "99 - Not known")]
        [Example(Description = "3")]
        public QualificationLevel CurrentQualificationLevel { get; set; }

        [DataType(DataType.DateTime)]
        [Example(Description = "2018-06-21T17:45:00")]
        public DateTime? DateQualificationLevelAchieved { get; set; }

        [StringLength(8)]
        public string LastLearningProvidersUKPRN { get; set; }

        [DataType(DataType.DateTime)]
        [Example(Description = "2018-06-21T17:45:00")]
        public DateTime? LastModifiedDate { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "0000000001")]
        public string LastModifiedTouchpointID { get; set; }

        [StringLength(10)]
        public string CreatedBy { get; set; }
    }
}
