using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace D365Plugins
{
    public class PreventDuplicateAccount : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(
                    typeof(IPluginExecutionContext));

            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(
                    typeof(ITracingService));

            // The InputParameters collection contains all the data passed
            // in the message request.
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.
                Entity entity = (Entity)context.InputParameters["Target"];

                // Obtain the IOrganizationService instance.
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(
                        typeof(IOrganizationServiceFactory));

                IOrganizationService service =
                    serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    // Validate message and entity.
                    if (context.MessageName != "Create" ||
                        context.PrimaryEntityName != "account")
                    {
                        return;
                    }

                    // Retrieve GSTIN.
                    string gstin =
                        entity.GetAttributeValue<string>("cr975_gstin");

                    // Allow creation if GSTIN is empty.
                    if (string.IsNullOrWhiteSpace(gstin))
                    {
                        return;
                    }

                    // Normalize incoming GSTIN.
                    gstin = gstin.Trim().ToUpperInvariant();

                    tracingService.Trace("Checking duplicate GSTIN: {0}", gstin);

                    // Create QueryExpression.
                    QueryExpression query = new QueryExpression("account")
                    {
                        ColumnSet = new ColumnSet("accountid"),
                        TopCount = 1
                    };

                    query.Criteria.AddCondition(
                        "cr975_gstin",
                        ConditionOperator.Equal,
                        gstin);

                    // Retrieve matching Accounts.
                    EntityCollection results =
                        service.RetrieveMultiple(query);

                    // Prevent duplicate Account creation.
                    if (results.Entities.Count > 0)
                    {
                        throw new InvalidPluginExecutionException(
                            "An Account with this GSTIN already exists.");
                    }

                    tracingService.Trace("GSTIN validation completed.");
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    tracingService.Trace(
                        "Organization Service Error: {0}", ex.ToString());

                    throw new InvalidPluginExecutionException(
                        "An error occurred while validating the GSTIN.", ex);
                }
                catch (InvalidPluginExecutionException)
                {
                    throw;
                }

                catch (Exception ex)
                {
                    tracingService.Trace(
                        "PreventDuplicateAccount: {0}", ex.ToString());

                    throw new InvalidPluginExecutionException(
                        "An error occurred while validating the GSTIN.", ex);
                }
            }
        }
    }
}
