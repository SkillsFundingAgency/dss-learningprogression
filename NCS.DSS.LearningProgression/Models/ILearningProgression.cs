using System;
using NCS.DSS.LearningProgression.Enumerations;

namespace NCS.DSS.LearningProgression.Models
{
    public interface ILearningProgression
    {
        string CreatedBy { get; set; }
        CurrentLearningStatus? CurrentLearningStatus { get; set; }
        QualificationLevel? CurrentQualificationLevel { get; set; }
        Guid? CustomerId { get; set; }
        DateTime? DateLearningStarted { get; set; }
        DateTime? DateProgressionRecorded { get; set; }
        DateTime? DateQualificationLevelAchieved { get; set; }
        string LastLearningProvidersUKPRN { get; set; }
        DateTime? LastModifiedDate { get; set; }
        string LastModifiedTouchpointID { get; set; }
        LearningHours? LearningHours { get; set; }
        Guid? LearningProgressionId { get; set; }
    }
}