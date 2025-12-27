using Microsoft.AspNetCore.Html;
using System.Net;

namespace QLDuLichRBAC_Upgrade.Utils
{
    public static class AlertHelper
    {
        // Helper method để escape HTML và prevent XSS
        private static string EscapeHtml(string text)
        {
            return WebUtility.HtmlEncode(text);
        }

        public static HtmlString Error(string message)
        {
            var safeMessage = EscapeHtml(message);
            return new HtmlString($@"
                <div class='custom-alert custom-alert-error' style='
                    position: fixed;
                    top: 20px;
                    right: 20px;
                    z-index: 9999;
                    min-width: 300px;
                    max-width: 500px;
                    padding: 15px 20px;
                    background: linear-gradient(135deg, #ff4757 0%, #ff6348 100%);
                    color: white;
                    border-radius: 10px;
                    box-shadow: 0 8px 20px rgba(255, 71, 87, 0.4);
                    animation: slideInRight 0.5s ease-out, fadeOut 0.5s ease-out 4.5s;
                    display: flex;
                    align-items: center;
                    font-family: Arial, sans-serif;'>
                    <div style='
                        width: 40px;
                        height: 40px;
                        background: rgba(255, 255, 255, 0.2);
                        border-radius: 50%;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        margin-right: 15px;
                        font-size: 24px;'>
                        ?
                    </div>
                    <div style='flex: 1;'>
                        <div style='font-weight: bold; font-size: 16px; margin-bottom: 5px;'>Error</div>
                        <div style='font-size: 14px; opacity: 0.95;'>{safeMessage}</div>
                    </div>
                    <button onclick='this.parentElement.remove()' style='
                        background: none;
                        border: none;
                        color: white;
                        font-size: 20px;
                        cursor: pointer;
                        padding: 0;
                        margin-left: 10px;
                        opacity: 0.7;
                        transition: opacity 0.3s;'
                        onmouseover='this.style.opacity=1'
                        onmouseout='this.style.opacity=0.7'>�</button>
                </div>
                <style>
                    @keyframes slideInRight {{
                        from {{ transform: translateX(400px); opacity: 0; }}
                        to {{ transform: translateX(0); opacity: 1; }}
                    }}
                    @keyframes fadeOut {{
                        to {{ opacity: 0; transform: translateX(400px); }}
                    }}
                </style>
                <script>
                    setTimeout(function() {{
                        var alerts = document.querySelectorAll('.custom-alert-error');
                        alerts.forEach(function(alert) {{ alert.remove(); }});
                    }}, 5000);
                </script>
            ");
        }

        public static HtmlString Success(string message)
        {
            var safeMessage = EscapeHtml(message);
            return new HtmlString($@"
                <div class='custom-alert custom-alert-success' style='
                    position: fixed;
                    top: 20px;
                    right: 20px;
                    z-index: 9999;
                    min-width: 300px;
                    max-width: 500px;
                    padding: 15px 20px;
                    background: linear-gradient(135deg, #2ecc71 0%, #27ae60 100%);
                    color: white;
                    border-radius: 10px;
                    box-shadow: 0 8px 20px rgba(46, 204, 113, 0.4);
                    animation: slideInRight 0.5s ease-out, fadeOut 0.5s ease-out 4.5s;
                    display: flex;
                    align-items: center;
                    font-family: Arial, sans-serif;'>
                    <div style='
                        width: 40px;
                        height: 40px;
                        background: rgba(255, 255, 255, 0.2);
                        border-radius: 50%;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        margin-right: 15px;
                        font-size: 24px;'>
                        ?
                    </div>
                    <div style='flex: 1;'>
                        <div style='font-weight: bold; font-size: 16px; margin-bottom: 5px;'>Success</div>
                        <div style='font-size: 14px; opacity: 0.95;'>{safeMessage}</div>
                    </div>
                    <button onclick='this.parentElement.remove()' style='
                        background: none;
                        border: none;
                        color: white;
                        font-size: 20px;
                        cursor: pointer;
                        padding: 0;
                        margin-left: 10px;
                        opacity: 0.7;
                        transition: opacity 0.3s;'
                        onmouseover='this.style.opacity=1'
                        onmouseout='this.style.opacity=0.7'>�</button>
                </div>
                <style>
                    @keyframes slideInRight {{
                        from {{ transform: translateX(400px); opacity: 0; }}
                        to {{ transform: translateX(0); opacity: 1; }}
                    }}
                    @keyframes fadeOut {{
                        to {{ opacity: 0; transform: translateX(400px); }}
                    }}
                </style>
                <script>
                    setTimeout(function() {{
                        var alerts = document.querySelectorAll('.custom-alert-success');
                        alerts.forEach(function(alert) {{ alert.remove(); }});
                    }}, 5000);
                </script>
            ");
        }

        public static HtmlString Warning(string message)
        {
            var safeMessage = EscapeHtml(message);
            return new HtmlString($@"
                <div class='custom-alert custom-alert-warning' style='
                    position: fixed;
                    top: 20px;
                    right: 20px;
                    z-index: 9999;
                    min-width: 300px;
                    max-width: 500px;
                    padding: 15px 20px;
                    background: linear-gradient(135deg, #f39c12 0%, #e67e22 100%);
                    color: white;
                    border-radius: 10px;
                    box-shadow: 0 8px 20px rgba(243, 156, 18, 0.4);
                    animation: slideInRight 0.5s ease-out, fadeOut 0.5s ease-out 4.5s;
                    display: flex;
                    align-items: center;
                    font-family: Arial, sans-serif;'>
                    <div style='
                        width: 40px;
                        height: 40px;
                        background: rgba(255, 255, 255, 0.2);
                        border-radius: 50%;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        margin-right: 15px;
                        font-size: 24px;'>
                        ?
                    </div>
                    <div style='flex: 1;'>
                        <div style='font-weight: bold; font-size: 16px; margin-bottom: 5px;'>Warning</div>
                        <div style='font-size: 14px; opacity: 0.95;'>{safeMessage}</div>
                    </div>
                    <button onclick='this.parentElement.remove()' style='
                        background: none;
                        border: none;
                        color: white;
                        font-size: 20px;
                        cursor: pointer;
                        padding: 0;
                        margin-left: 10px;
                        opacity: 0.7;
                        transition: opacity 0.3s;'
                        onmouseover='this.style.opacity=1'
                        onmouseout='this.style.opacity=0.7'>�</button>
                </div>
                <style>
                    @keyframes slideInRight {{
                        from {{ transform: translateX(400px); opacity: 0; }}
                        to {{ transform: translateX(0); opacity: 1; }}
                    }}
                    @keyframes fadeOut {{
                        to {{ opacity: 0; transform: translateX(400px); }}
                    }}
                </style>
                <script>
                    setTimeout(function() {{
                        var alerts = document.querySelectorAll('.custom-alert-warning');
                        alerts.forEach(function(alert) {{ alert.remove(); }});
                    }}, 5000);
                </script>
            ");
        }

        public static HtmlString Info(string message)
        {
            var safeMessage = EscapeHtml(message);
            return new HtmlString($@"
                <div class='custom-alert custom-alert-info' style='
                    position: fixed;
                    top: 20px;
                    right: 20px;
                    z-index: 9999;
                    min-width: 300px;
                    max-width: 500px;
                    padding: 15px 20px;
                    background: linear-gradient(135deg, #3498db 0%, #2980b9 100%);
                    color: white;
                    border-radius: 10px;
                    box-shadow: 0 8px 20px rgba(52, 152, 219, 0.4);
                    animation: slideInRight 0.5s ease-out, fadeOut 0.5s ease-out 4.5s;
                    display: flex;
                    align-items: center;
                    font-family: Arial, sans-serif;'>
                    <div style='
                        width: 40px;
                        height: 40px;
                        background: rgba(255, 255, 255, 0.2);
                        border-radius: 50%;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        margin-right: 15px;
                        font-size: 24px;'>
                        ?
                    </div>
                    <div style='flex: 1;'>
                        <div style='font-weight: bold; font-size: 16px; margin-bottom: 5px;'>Info</div>
                        <div style='font-size: 14px; opacity: 0.95;'>{safeMessage}</div>
                    </div>
                    <button onclick='this.parentElement.remove()' style='
                        background: none;
                        border: none;
                        color: white;
                        font-size: 20px;
                        cursor: pointer;
                        padding: 0;
                        margin-left: 10px;
                        opacity: 0.7;
                        transition: opacity 0.3s;'
                        onmouseover='this.style.opacity=1'
                        onmouseout='this.style.opacity=0.7'>�</button>
                </div>
                <style>
                    @keyframes slideInRight {{
                        from {{ transform: translateX(400px); opacity: 0; }}
                        to {{ transform: translateX(0); opacity: 1; }}
                    }}
                    @keyframes fadeOut {{
                        to {{ opacity: 0; transform: translateX(400px); }}
                    }}
                </style>
                <script>
                    setTimeout(function() {{
                        var alerts = document.querySelectorAll('.custom-alert-info');
                        alerts.forEach(function(alert) {{ alert.remove(); }});
                    }}, 5000);
                </script>
            ");
        }
    }
}
