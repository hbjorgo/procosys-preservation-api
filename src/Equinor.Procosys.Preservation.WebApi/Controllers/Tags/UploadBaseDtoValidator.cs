﻿using System.IO;
using System.Linq;
using Equinor.Procosys.Preservation.Domain;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Equinor.Procosys.Preservation.WebApi.Controllers.Tags
{
    public class UploadBaseDtoValidator<T> : AbstractValidator<T> where T: UploadBaseDto
    {
        public UploadBaseDtoValidator(IOptionsMonitor<AttachmentOptions> options)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(x => x)
                .NotNull();

            RuleFor(x => x.File)
                .NotNull();
            
            RuleFor(x => x.File.FileName)
                .NotEmpty()
                .WithMessage("Filename not given!")
                .MaximumLength(Attachment.FileNameLengthMax)
                .WithMessage($"Filename to long! Max {Attachment.FileNameLengthMax} characters")
                .Must(BeAValidFile)
                .WithMessage(x => $"File {x.File.FileName} is not a valid file for upload!")
                .When(x => x.File != null);
            
            RuleFor(x => x.File.Length)
                .Must(BeSmallerThanMaxSize)
                .When(x => x.File != null)
                .WithMessage($"Maximum file size is {options.CurrentValue.MaxSizeMb}MB!");

            bool BeAValidFile(string fileName)
            {
                var suffix = Path.GetExtension(fileName?.ToLower());
                return suffix != null && options.CurrentValue.ValidFileSuffixes.Contains(suffix) && fileName?.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
            }
            
            bool BeSmallerThanMaxSize(long fileSizeInBytes)
            {
                var maxSizeInBytes = options.CurrentValue.MaxSizeMb * 1024 * 1024;
                return fileSizeInBytes < maxSizeInBytes;
            }
        }
    }
}
