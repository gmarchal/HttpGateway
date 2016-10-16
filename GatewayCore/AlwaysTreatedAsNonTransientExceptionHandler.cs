using System;
using Microsoft.ServiceFabric.Services.Communication.Client;

namespace Iridium.GatewayCore
{    
    /// <summary>
    /// Defines the exception handler for the retry logic.
    /// </summary>
    public class AlwaysTreatedAsNonTransientExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// Tries to handle the exception.
        /// </summary>
        /// <param name="exceptionInformation">
        /// The exception information.
        /// </param>
        /// <param name="retrySettings">
        /// The retry settings.
        /// </param>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// <b>true</b> if the exception was handled; otherwise, <b>false</b>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Either the exception information of the retry settings is null.
        /// </exception>
        public bool TryHandleException(
            ExceptionInformation exceptionInformation,
            OperationRetrySettings retrySettings,
            out ExceptionHandlingResult result)
        {
            if (exceptionInformation == null)
            {
                throw new ArgumentNullException("exceptionInformation");
            }

            if (retrySettings == null)
            {
                throw new ArgumentNullException("retrySettings");
            }

            result = new ExceptionHandlingRetryResult(
                exceptionInformation.Exception,
                false,
                retrySettings,
                retrySettings.DefaultMaxRetryCount);

            return true;
        }
    }
}