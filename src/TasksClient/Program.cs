const string TOKENS_PATH = @"secrets\tokens";
const string CREDENTIALS_PATH = @"secrets\google_api_credentials.json";
const string USER_ACCOUNT_VARNAME = "MAIN_GMAIL_ADDRESS";
const string PUBLIC_APP_NAME_PATH = @"secrets\app_public_name.txt";

string userAccount = Environment.GetEnvironmentVariable(
	variable: USER_ACCOUNT_VARNAME,
	target: EnvironmentVariableTarget.User
) ?? throw new Exception(string.Format("{0} environment variable is not set.", USER_ACCOUNT_VARNAME));

string appName = File.ReadAllText(PUBLIC_APP_NAME_PATH).Trim();

Google.Apis.Auth.OAuth2.UserCredential credential;
using (var stream = new FileStream(CREDENTIALS_PATH, FileMode.Open, FileAccess.Read))
{
    credential = Google.Apis.Auth.OAuth2.GoogleWebAuthorizationBroker.AuthorizeAsync(
        clientSecrets: Google.Apis.Auth.OAuth2.GoogleClientSecrets.FromStream(stream).Secrets,
		scopes: [Google.Apis.Tasks.v1.TasksService.Scope.Tasks],
		user: userAccount,
        taskCancellationToken: CancellationToken.None,
		dataStore: new Google.Apis.Util.Store.FileDataStore(TOKENS_PATH, true)).Result;
}

// Use the credential object when creating the TasksService.
var service = new Google.Apis.Tasks.v1.TasksService(new Google.Apis.Services.BaseClientService.Initializer()
{
    HttpClientInitializer = credential,
    ApplicationName = appName,
});

// Create a new task
var newTask = new Google.Apis.Tasks.v1.Data.Task() {
	Title = "My new task",
	Notes = "Please complete me",
};

// Specify the task list where the task will be created. 
// You can get the task list ID from the Google Tasks UI.
string taskListId = "@default"; // Use the default task list

// Create the task
Google.Apis.Tasks.v1.TasksResource.InsertRequest request = service.Tasks.Insert(newTask, taskListId);
Google.Apis.Tasks.v1.Data.Task createdTask = request.Execute();

Console.WriteLine($"Created task {createdTask.Id}");

// class Options
// {
//     [Option('t', "task", Required = true, HelpText = "Task to add to Google Tasks.")]
//     public string Task { get; set; }
// }