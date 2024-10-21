using NCS.DSS.LearningProgression.Models;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.LearningProgression.Validators
{
    public class Validate : IValidate
    {
        public List<ValidationResult> ValidateResource(ILearningProgression learningProgressionResource)
        {
            var context = new ValidationContext(learningProgressionResource, null, null);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(learningProgressionResource, context, results, true);
            ValidateLearningProgressionRules(learningProgressionResource, results);

            return results;
        }

        private void ValidateLearningProgressionRules(ILearningProgression learningProgressionResource, List<ValidationResult> results)
        {
            if (learningProgressionResource == null)
            {
                return;
            }

            if (!learningProgressionResource.CustomerId.HasValue)
            {
                results.Add(new ValidationResult("CustomerId is mandatory.", new[] { "CustomerId" }));
            }

            if (learningProgressionResource.DateProgressionRecorded.HasValue)
            {
                if (learningProgressionResource.DateProgressionRecorded.Value > DateTime.UtcNow)
                {
                    results.Add(new ValidationResult("DateProgressionRecorded must be less than or equal to now.", new[] { "DateProgressionRecorded" }));
                }
            }

            if (learningProgressionResource.CurrentLearningStatus.HasValue)
            {
                if (learningProgressionResource.CurrentLearningStatus.Value == ReferenceData.CurrentLearningStatus.InLearning)
                {
                    if (!Enum.IsDefined(typeof(ReferenceData.CurrentLearningStatus), learningProgressionResource.CurrentLearningStatus.Value))
                    {
                        results.Add(new ValidationResult("CurrentLearningStatus must be a valid current Learning Status.", new[] { "CurrentLearningStatus" }));
                    }
                }
                else
                {
                    if (!Enum.IsDefined(typeof(ReferenceData.CurrentLearningStatus), learningProgressionResource.CurrentLearningStatus.Value))
                    {
                        results.Add(new ValidationResult("CurrentLearningStatus must be a valid current Learning Status.", new[] { "CurrentLearningStatus" }));
                    }
                }
            }

            if (learningProgressionResource.CurrentQualificationLevel.HasValue)
            {
                if (!Enum.IsDefined(typeof(ReferenceData.QualificationLevel), learningProgressionResource.CurrentQualificationLevel.Value))
                {
                    results.Add(new ValidationResult("CurrentQualificationLevel must be a valid current Qualification Level.", new[] { "CurrentQualificationLevel" }));
                }
            }
            // ==== end of mandatory fields

            if (learningProgressionResource.LearningHours.HasValue)
            {
                if (!Enum.IsDefined(typeof(ReferenceData.LearningHours), learningProgressionResource.LearningHours.Value))
                {
                    {
                        results.Add(new ValidationResult("LearningHours must have a valid Learning Hours value.", new[] { "LearningHours" }));
                    }
                }
            }

            if (learningProgressionResource.CurrentLearningStatus.HasValue)
            {
                if (learningProgressionResource.CurrentLearningStatus == ReferenceData.CurrentLearningStatus.InLearning)
                {
                    if (!learningProgressionResource.DateLearningStarted.HasValue)
                    {
                        results.Add(new ValidationResult("DateLearningStarted must have a value when Current Learning Status is InLearning.", new[] { "DateLearningStarted" }));
                    }
                    else
                    {
                        if (learningProgressionResource.DateLearningStarted.Value > DateTime.UtcNow)
                        {
                            {
                                results.Add(new ValidationResult("DateLearningStarted must be less than or equal to now.", new[] { "DateLearningStarted" }));
                            }
                        }
                    }
                }
                else
                {
                    if (learningProgressionResource.DateLearningStarted.HasValue)
                    {
                        if (learningProgressionResource.DateLearningStarted.Value > DateTime.UtcNow)
                        {
                            results.Add(new ValidationResult("DateLearningStarted must be less than or equal to now.", new[] { "DateLearningStarted" }));
                        }
                    }
                }
            }
            else
            {
                if (learningProgressionResource.DateLearningStarted.HasValue)
                {
                    if (learningProgressionResource.DateLearningStarted.Value > DateTime.UtcNow)
                    {
                        results.Add(new ValidationResult("DateLearningStarted must be less than or equal to now.", new[] { "DateLearningStarted" }));
                    }
                }
            }

            if (learningProgressionResource.CurrentLearningStatus.HasValue)
            {
                if (learningProgressionResource.CurrentLearningStatus == ReferenceData.CurrentLearningStatus.InLearning)
                {
                    if (!learningProgressionResource.LearningHours.HasValue)
                    {
                        results.Add(new ValidationResult("LearningHours must have a value when Current Learning Status is InLearning.", new[] { "LearningHours" }));
                    }
                }
            }

            if (learningProgressionResource.CurrentQualificationLevel.HasValue && learningProgressionResource.CurrentQualificationLevel.Value < ReferenceData.QualificationLevel.NoQualifications)
            {
                if (!learningProgressionResource.DateQualificationLevelAchieved.HasValue)
                {
                    results.Add(new ValidationResult("DateQualificationLevelAchieved is required when QualificationLevel < NoQualification (99).", new[] { "DateQualificationLevelAchieved" }));
                }
                else
                {
                    if (learningProgressionResource.DateQualificationLevelAchieved.HasValue)
                    {
                        if (learningProgressionResource.DateQualificationLevelAchieved.Value > DateTime.UtcNow)
                        {
                            results.Add(new ValidationResult("DateQualificationLevelAchieved must be less than or equal to now.", new[] { "DateQualificationLevelAchieved" }));
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(learningProgressionResource.LastLearningProvidersUKPRN))
            {
                if (int.TryParse(learningProgressionResource.LastLearningProvidersUKPRN, out int value))
                {
                    if (value < 10000000 && value > 99999999)
                    {
                        results.Add(new ValidationResult("LastLearningProvidersUKPRN must be a value between 10000000 - 99999999.", new[] { "LastLearningProvidersUKPRN" }));
                    }
                }
                else
                {
                    results.Add(new ValidationResult("LastLearningProvidersUKPRN must be a Number.", new[] { "LastLearningProvidersUKPRN" }));
                }
            }
        }
    }
}