﻿using NCS.DSS.LearningProgression.Models;
using System;
using System.Collections.Generic;
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
                results.Add(new ValidationResult("CustomerId is mandatory.", new[] { "DateProgressionRecorded" }));
            }

            if (!learningProgressionResource.DateProgressionRecorded.HasValue)
            {
                results.Add(new ValidationResult("Date Progression Recorded is mandatory.", new[] { "DateProgressionRecorded" }));
            }
            else
            {
                if (learningProgressionResource.DateProgressionRecorded.Value > DateTime.UtcNow)
                {
                    if (learningProgressionResource.DateProgressionRecorded.Value > DateTime.UtcNow)
                    {
                        results.Add(new ValidationResult("Date And Time must be less the current date/time", new[] { "DateProgressionRecorded" }));
                    }
                }
            }

            if (!learningProgressionResource.CurrentLearningStatus.HasValue)
            {
                results.Add(new ValidationResult("Current Learning Status is mandatory.", new[] { "CurrentLearningStatus" }));
            }
            else
            {
                if (learningProgressionResource.CurrentLearningStatus.Value == Enumerations.CurrentLearningStatus.InLearning)
                {
                    if (!Enum.IsDefined(typeof(Enumerations.CurrentLearningStatus), learningProgressionResource.CurrentLearningStatus.Value))
                    {
                        results.Add(new ValidationResult("Current learning status must be a valid CurrentLearningStatus.", new[] { "CurrentLearningStatus" }));
                    }
                }
            }

            if (!learningProgressionResource.CurrentQualificationLevel.HasValue)
            {
                results.Add(new ValidationResult("Current Qualification Level is a mandatory field.", new[] { "CurrentQualificationLevel" }));
            }
            else
            {
                if (!Enum.IsDefined(typeof(Enumerations.QualificationLevel), learningProgressionResource.CurrentQualificationLevel.Value))
                {
                    results.Add(new ValidationResult("Please supply a valid value for CurrentQualificationLevel", new[] { "CurrentQualificationLevel" }));
                }
            }
            // ==== end of mandatory fields


            if (learningProgressionResource.CurrentLearningStatus.HasValue)
            {
                if (learningProgressionResource.CurrentLearningStatus == Enumerations.CurrentLearningStatus.InLearning)
                {
                    if (!learningProgressionResource.DateLearningStarted.HasValue)
                    {
                        results.Add(new ValidationResult("Date Learning Started must have a value when Current Learning Status is InLearning.", new[] { "DateLearningStarted" }));
                    }
                    else
                    {
                        if (learningProgressionResource.DateLearningStarted.Value > DateTime.UtcNow)
                        {
                            {
                                results.Add(new ValidationResult("Date Learning Started must have a value when Current Learning Status is InLearning.", new[] { "DateLearningStarted" }));
                            }
                        }
                    }
                }
            }

            if (learningProgressionResource.CurrentQualificationLevel.HasValue)
            {
                if (learningProgressionResource.CurrentQualificationLevel.Value < Enumerations.QualificationLevel.NoQualifications)
                {
                    if (!learningProgressionResource.DateQualificationLevelAchieved.HasValue)
                    {
                        results.Add(new ValidationResult("When QualificationLevel < NoQualification (99) a valid value is required for DateQualificationLevelAchieved.", new[] { "DateQualificationLevelAchieved" }));
                    }
                    else
                    {
                        if (learningProgressionResource.DateLearningStarted.Value > DateTime.UtcNow)
                        {
                            results.Add(new ValidationResult("When QualificationLevel is NoQualification DateQualificationLevelAchieved must be before today.", new[] { "DateQualificationLevelAchieved" }));
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(learningProgressionResource.LastLearningProvidersUKPRN))
            {
                if (learningProgressionResource.LastLearningProvidersUKPRN.Trim().Length != 8)
                {
                    results.Add(new ValidationResult("LastLearningProvidersUKPRN must be exactly 8 numeric digits in length.", new[] { "LastLearningProvidersUKPRN" }));
                }
                else
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
                        results.Add(new ValidationResult("LastLearningProvidersUKPRN must be a Number (and between 10000000 - 99999999).", new[] { "LastLearningProvidersUKPRN" }));
                    }
                }
            }
        }
    }
}