namespace Nop.Core.Domain.Gdpr
{
    /// <summary>
    /// Represents a GDPR request type
    /// </summary>
    public enum GdprRequestType
    {
        /// <summary>
        /// Consent (agree)
        /// </summary>
        ConsentAgree = 1,

        /// <summary>
        /// Consent (disagree)
        /// </summary>
        ConsentDisagree = 5,

        /// <summary>
        /// Export data
        /// </summary>
        ExportData = 10,

        /// <summary>
        /// Delete customer
        /// </summary>
        DeleteCustomer = 15,

        /// <summary>
        /// User changed first name
        /// </summary>
        FirstNameChanged = 20,

        /// <summary>
        /// User changed last name
        /// </summary>
        LastNameChanged = 21,

        /// <summary>
        /// User changed gender
        /// </summary>
        GenderChanged = 22,

        /// <summary>
        /// User changed date of birth
        /// </summary>
        DateOfBirthChanged = 23,

        /// <summary>
        /// User changed email
        /// </summary>
        EmailChanged = 24,

        /// <summary>
        /// User changed company name
        /// </summary>
        CompanyChanged = 25

    }
}
