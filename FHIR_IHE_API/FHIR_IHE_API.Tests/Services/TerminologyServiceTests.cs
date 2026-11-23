
using FHIR_IHE_API.Services;
using Hl7.Fhir.Model;
using Xunit;

namespace FHIR_IHE_API.Tests.Services
{
    public class TerminologyServiceTests
    {
        private readonly TerminologyService _terminologyService;

        public TerminologyServiceTests()
        {
            // Create a TerminologyService instance for testing.
            _terminologyService = new TerminologyService();
        }

        [Fact]
        public void ValidateCodeDisplay_ValidCodeAndDisplay_ReturnsValid()
        {
            // Arrange
            var system = "http://hl7.org/fhir/administrative-gender";
            var code = "male";
            var display = "Male";

            // Act
            var result = _terminologyService.ValidateCodeDisplay(
                system, code, display);

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal("male", result.Code);
            Assert.Equal("Male", result.ExpectedDisplay);
            Assert.Equal("Male", result.ProvidedDisplay);
        }

        [Fact]
        public void ValidateCodeDisplay_IncorrectDisplay_ReturnsInvalid()
        {
            // Arrange
            var system = "http://hl7.org/fhir/administrative-gender";
            var code = "male";
            var display = "Female";

            // Act
            var result = _terminologyService.ValidateCodeDisplay(
                system, code, display);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Female", result.ProvidedDisplay);
            Assert.Equal("Male", result.ExpectedDisplay);
        }

        [Fact]
        public void ValidateCodeDisplay_ValidFemaleCodeAndDisplay_ReturnsValid()
        {
            // Arrange
            var system = "http://hl7.org/fhir/administrative-gender";
            var code = "female";
            var display = "Female";

            // Act
            var result = _terminologyService.ValidateCodeDisplay(
                system, code, display);

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal("Female", result.ExpectedDisplay);
        }

        [Fact]
        public void ValidateCodeDisplay_InvalidCode_ReturnsInvalid()
        {
            // Arrange
            var system = "http://hl7.org/fhir/administrative-gender";
            var code = "invalid";
            var display = "Invalid";

            // Act
            var result = _terminologyService.ValidateCodeDisplay(
                system, code, display);

            // Assert
            Assert.False(result.IsValid);
            Assert.Null(result.ExpectedDisplay);
            Assert.Equal("Invalid", result.ProvidedDisplay);
        }

        [Fact]
        public void ValidateCodeDisplay_NoDisplayProvided_ValidatesCodeOnly()
        {
            // Arrange
            var system = "http://hl7.org/fhir/administrative-gender";
            var code = "other";

            // Act
            var result = _terminologyService.ValidateCodeDisplay(
                system, code, null);

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal("Other", result.ExpectedDisplay);
            Assert.Null(result.ProvidedDisplay);
        }

        [Fact]
        public void ValidateCodeDisplay_CodeWithIncorrectCase_ReturnsInvalid()
        {
            // Arrange
            var system = "http://hl7.org/fhir/administrative-gender";
            var code = "Male";
            var display = "Male";

            // Act
            var result = _terminologyService.ValidateCodeDisplay(
                system, code, display);

            // Assert
            Assert.False(result.IsValid);
            Assert.Null(result.ExpectedDisplay);
        }

        [Fact]
        public void ValidateCoding_ValidCoding_ReturnsValid()
        {
            // Arrange
            var coding = new Coding
            {
                System = "http://hl7.org/fhir/administrative-gender",
                Code = "male",
                Display = "Male"
            };

            // Act
            var result = _terminologyService.ValidateCoding(coding);

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal("male", result.Code);
            Assert.Equal("Male", result.ExpectedDisplay);
        }

        [Fact]
        public void ValidateCoding_IncorrectDisplay_ReturnsInvalid()
        {
            // Arrange
            var coding = new Coding
            {
                System = "http://hl7.org/fhir/administrative-gender",
                Code = "male",
                Display = "Female"
            };

            // Act
            var result = _terminologyService.ValidateCoding(coding);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Female", result.ProvidedDisplay);
            Assert.Equal("Male", result.ExpectedDisplay);
        }

        [Fact]
        public void ValidateCoding_InvalidCode_ReturnsInvalid()
        {
            // Arrange
            var coding = new Coding
            {
                System = "http://hl7.org/fhir/administrative-gender",
                Code = "invalid",
                Display = "Invalid"
            };

            // Act
            var result = _terminologyService.ValidateCoding(coding);

            // Assert
            Assert.False(result.IsValid);
            Assert.Null(result.ExpectedDisplay);
        }

        [Fact]
        public void ValidateCoding_MissingSystem_ReturnsInvalid()
        {
            // Arrange
            var coding = new Coding
            {
                Code = "male",
                Display = "Male"
            };

            // Act
            var result = _terminologyService.ValidateCoding(coding);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("system", result.Message);
        }

        [Fact]
        public void ValidateCoding_MissingCode_ReturnsInvalid()
        {
            // Arrange
            var coding = new Coding
            {
                System = "http://hl7.org/fhir/administrative-gender",
                Display = "Male"
            };

            // Act
            var result = _terminologyService.ValidateCoding(coding);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("code", result.Message);
        }

        [Fact]
        public void ValidateCodeDisplay_UnknownSystem_ReturnsSystemNotFound()
        {
            var result = _terminologyService.ValidateCodeDisplay(
                "http://example.org/unknown",
                "male",
                "Male");

            Assert.False(result.IsValid);
            Assert.False(result.SystemFound);
            Assert.False(result.CanValidateCode);
        }

        [Fact]
        public void ValidateCodeDisplay_UnknownCode_ReturnsCodeNotFound()
        {
            var result = _terminologyService.ValidateCodeDisplay(
                "http://hl7.org/fhir/administrative-gender",
                "invalid",
                "Invalid");

            Assert.False(result.IsValid);
            Assert.True(result.SystemFound);
            Assert.True(result.CanValidateCode);
        }

        [Fact]
        public void ValidateCodeDisplay_KnownSystemAndValidCode_ReturnsValid()
        {
            var result = _terminologyService.ValidateCodeDisplay(
                "http://hl7.org/fhir/administrative-gender",
                "male",
                "Male");

            Assert.True(result.IsValid);
            Assert.True(result.SystemFound);
            Assert.True(result.CanValidateCode);
        }

        [Fact]
        public void CanValidateCode_CompleteCodeSystem_ReturnsTrue()
        {
            var result = _terminologyService.CanValidateCode(
                "http://hl7.org/fhir/administrative-gender");

            Assert.True(result);
        }


        [Fact]
        public void ExpandValueSet_ShouldReturnFourAdministrativeGenderCodes()
        {
            // Arrange
            var service = new TerminologyService();

            // Act
            var result = service.ExpandValueSet("administrative-gender");

            // Assert
            Assert.IsType<ValueSet>(result);
            Assert.Equal("ValueSet", result.TypeName);
            Assert.NotNull(result.Expansion);
            Assert.Equal(4, result.Expansion.Total);
            Assert.Equal(4, result.Expansion.Contains.Count);
        }

        [Theory]
        [InlineData("male")]
        [InlineData("female")]
        [InlineData("other")]
        [InlineData("unknown")]
        public void IsCodeInValueSet_ShouldReturnTrue_ForValidAdministrativeGenderCodes(
            string code)
        {
            // Arrange
            var service = new TerminologyService();

            // Act
            var result = service.IsCodeInValueSet(
                "administrative-gender",
                "http://hl7.org/fhir/administrative-gender",
                code);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsCodeInValueSet_ShouldReturnFalse_ForInvalidCode()
        {
            // Arrange
            var service = new TerminologyService();

            // Act
            var result = service.IsCodeInValueSet(
                "administrative-gender",
                "http://hl7.org/fhir/administrative-gender",
                "invalid");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsCodeInValueSet_ShouldReturnFalse_ForUnknownValueSet()
        {
            // Arrange
            var service = new TerminologyService();

            // Act
            var result = service.IsCodeInValueSet(
                "unknown-valueset",
                "http://hl7.org/fhir/administrative-gender",
                "male");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsCodeInValueSet_ShouldReturnFalse_ForWrongCodeSystem()
        {
            // Arrange
            var service = new TerminologyService();

            // Act
            var result = service.IsCodeInValueSet(
                "administrative-gender",
                "http://example.org/unknown-system",
                "male");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateValueSetCode_ShouldReturnTrue_ForValidCodeAndDisplay()
        {
            // Arrange
            var service = new TerminologyService();

            // Act
            var result = service.ValidateValueSetCode(
                "administrative-gender",
                "http://hl7.org/fhir/administrative-gender",
                "male",
                "Male");

            // Assert
            Assert.True(result.IsValid);
            Assert.True(result.ValueSetFound);
            Assert.True(result.SystemFound);
            Assert.Equal("male", result.Code);
            Assert.Equal("Male", result.ExpectedDisplay);
        }

        [Fact]
        public void ValidateValueSetCode_ShouldReturnFalse_ForInvalidCode()
        {
            // Arrange
            var service = new TerminologyService();

            // Act
            var result = service.ValidateValueSetCode(
                "administrative-gender",
                "http://hl7.org/fhir/administrative-gender",
                "invalid");

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.ValueSetFound);
            Assert.True(result.SystemFound);
        }

        [Fact]
        public void ValidateValueSetCode_ShouldReturnFalse_ForIncorrectDisplay()
        {
            // Arrange
            var service = new TerminologyService();

            // Act
            var result = service.ValidateValueSetCode(
                "administrative-gender",
                "http://hl7.org/fhir/administrative-gender",
                "male",
                "Female");

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Male", result.ExpectedDisplay);
        }

        [Fact]
        public void ValidateValueSetCode_ShouldReturnFalse_ForUnknownSystem()
        {
            // Arrange
            var service = new TerminologyService();

            // Act
            var result = service.ValidateValueSetCode(
                "administrative-gender",
                "http://example.org/unknown-system",
                "male");

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.ValueSetFound);
            Assert.False(result.SystemFound);
        }

        [Fact]
        public void ValidateValueSetCode_ShouldIndicateMissingValueSet()
        {
            // Arrange
            var service = new TerminologyService();

            // Act
            var result = service.ValidateValueSetCode(
                "unknown-valueset",
                "http://hl7.org/fhir/administrative-gender",
                "male");

            // Assert
            Assert.False(result.IsValid);
            Assert.False(result.ValueSetFound);
        }
    }
}

