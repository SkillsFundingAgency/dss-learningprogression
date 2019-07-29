using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.LearningProgression.Validators
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(Models.LearningProgression learningProgression);
    }
}
