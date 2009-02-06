﻿using System;
using System.Collections.Generic;
using ServiceStack.Common.Extensions;

namespace ServiceStack.Validation
{
	/// <summary>
	/// Encapsulates a validation result.
	/// </summary>
	public class ValidationResult
	{
		public static ValidationResult Success
		{
			get
			{
				return new ValidationResult();
			}
		}

		/// <summary>
		/// Gets or sets the success code.
		/// </summary>
		/// <value>The success code.</value>
		public string SuccessCode
		{
			get;
			protected set;
		}

		/// <summary>
		/// Gets or sets the error code.
		/// </summary>
		/// <value>The error code.</value>
		public string ErrorCode
		{
			get;
			protected set;
		}

		/// <summary>
		/// Gets or sets the success message.
		/// </summary>
		/// <value>The success message.</value>
		public string SuccessMessage { get; set; }

		/// <summary>
		/// Gets or sets the error message.
		/// </summary>
		/// <value>The error message.</value>
		public string ErrorMessage { get; set; }

		public virtual string Message
		{
			get
			{
				return Errors.Count > 0 ? ErrorMessage : SuccessMessage;
			}
		}


		/// <summary>
		/// The errors generated by the validation.
		/// </summary>
		public IList<ValidationError> Errors
		{
			get;
			protected set;
		}

		/// <summary>
		/// Returns True if the validation was successful (errors list is empty).
		/// </summary>
		public virtual bool IsValid
		{
			get
			{
				return this.Errors.Count == 0;
			}
		}

		/// <summary>
		/// Constructs a new ValidationResult
		/// </summary>
		public ValidationResult()
			: this(new List<ValidationError>())
		{
		}

		/// <summary>
		/// Constructs a new ValidationResult
		/// </summary>
		/// <param name="errors">A list of validation results</param>
		public ValidationResult(IList<ValidationError> errors) : this(errors, null, null) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ValidationResult"/> class.
		/// </summary>
		/// <param name="errors">The errors.</param>
		/// <param name="successCode">The success code.</param>
		/// <param name="errorCode">The error code.</param>
		public ValidationResult(IList<ValidationError> errors, string successCode, string errorCode)
		{
			this.Errors = errors ?? new List<ValidationError>();
			if (successCode != null)
			{
				this.SuccessCode = successCode;
				this.SuccessMessage = successCode.SplitCamelCase();
			}
			if (errorCode != null)
			{
				this.ErrorCode = errorCode;
			}
			else
			{
				if (this.Errors.Count > 0)
				{
					this.ErrorCode = this.Errors[0].ErrorCode;
					this.ErrorMessage = this.Errors[0].ErrorMessage;
				}
			}
			
			if (this.ErrorMessage == null && this.ErrorCode != null)
			{
				this.ErrorMessage = this.ErrorCode.SplitCamelCase();
			}
		}
	}
}