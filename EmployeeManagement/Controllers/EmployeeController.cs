using EmployeeManagement.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace EmployeeManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class EmployeeController : ControllerBase
    {

        // Cosmos DB details, In real use cases, these details should be configured in secure configuraion file.
        private readonly string CosmosDBAccountUri = "https://myapp-license-db.documents.azure.com:443/";
        private readonly string CosmosDBAccountPrimaryKey = "pvLGdvW1PYgSQmGijbn3OeX6KWy9Un0tkWHQzjfYfAtqcamJ6KJkGVAb5AEPXDsPEYS1UPpfXMZJACDbgXHHwg==";
        private readonly string CosmosDbName = "myapp-license-db";
        private readonly string CosmosDbContainerName = "Employees";


        /// <summary>
        /// Commom Container Client, you can also pass the configuration paramter dynamically.
        /// </summary>
        /// <returns> Container Client </returns>
        private Container ContainerClient()
        {

            CosmosClient cosmosDbClient = new CosmosClient(CosmosDBAccountUri, CosmosDBAccountPrimaryKey);
            Container containerClient = cosmosDbClient.GetContainer(CosmosDbName, CosmosDbContainerName);
            return containerClient;

        }


        [HttpPost]
        public async Task<IActionResult> AddEmployee(EmployeeModel employee)
        {
            try
            {
                var container = ContainerClient();
                var response = await container.CreateItemAsync(employee, new PartitionKey(employee.EmployeeCountryName));

                return Ok(response);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeDetails()
        {
            try
            {
                var container = ContainerClient();
                var sqlQuery = "SELECT * FROM c";
                QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                FeedIterator<EmployeeModel> queryResultSetIterator = container.GetItemQueryIterator<EmployeeModel>(queryDefinition);


                List<EmployeeModel> employees = new List<EmployeeModel>();

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<EmployeeModel> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (EmployeeModel employee in currentResultSet)
                    {
                        employees.Add(employee);
                    }
                }

                return Ok(employees);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }
        [HttpGet]
        public async Task<IActionResult> GetEmployeeDetailsById(string employeeId, string partitionKey)
        {

            try
            {
                var container = ContainerClient();
                ItemResponse<EmployeeModel> response = await container.ReadItemAsync<EmployeeModel>(employeeId, new PartitionKey(partitionKey));
                return Ok(response.Resource);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }
        [HttpPut]
        public async Task<IActionResult> UpdateEmployee(EmployeeModel emp, string partitionKey)
        {

            try
            {

                var container = ContainerClient();
                ItemResponse<EmployeeModel> res = await container.ReadItemAsync<EmployeeModel>(emp.id, new PartitionKey(partitionKey));

                //Get Existing Item
                var existingItem = res.Resource;

                //Replace existing item values with new values 
                existingItem.EmployeeFullName = emp.EmployeeFullName;
                existingItem.EmployeeCountryName = emp.EmployeeCountryName;
                existingItem.EmployeeEmail = emp.EmployeeEmail;
                existingItem.AgencyName = emp.AgencyName;
                existingItem.LoginPassword = emp.LoginPassword;

                var updateRes = await container.ReplaceItemAsync(existingItem, emp.id, new PartitionKey(partitionKey));

                return Ok(updateRes.Resource);

            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }

        [HttpDelete]
        public async Task<IActionResult> DeleteEmployee(string empId, string partitionKey)
        {

            try
            {

                var container = ContainerClient();
                var response = await container.DeleteItemAsync<EmployeeModel>(empId, new PartitionKey(partitionKey));
                return Ok(response.StatusCode);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

    }
}