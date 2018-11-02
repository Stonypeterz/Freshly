public class ResponseObject
{
    public Alerts Alert { get; set; }
    public string Title { get; set; }
    public string Msg { get; set; }

    public ResponseObject(Alerts type, string title, string msg)
    {
        Alert = type;
        Title = title;
        Msg = msg;
    }

    public ResponseObject():this(Alerts.danger, "Event information", "No data to display.") { }
}
