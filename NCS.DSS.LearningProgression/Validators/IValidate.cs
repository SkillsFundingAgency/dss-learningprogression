using NCS.DSS.LearningProgression.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.LearningProgression.Validators
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(ILearningProgression learningProgression);
    }
}
