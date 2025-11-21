namespace sims_web_app.Data.Model;



    public class LogOverview
    {
        public string logId { get; set; }
        public string message { get; set; }
        public string timestamp { get; set; }
        public string severity { get; set; }
        public bool selected { get; set; }
    }

