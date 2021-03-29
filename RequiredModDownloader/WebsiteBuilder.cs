using System;
using System.IO;

namespace RequiredModInstaller
{
    public class WebsiteBuilder
    {

        public void CreateWebsite(String[] names, String[] source, String exportPath)
        {
            String websiteSource = $"<!DOCTYPE html>\n<html>\n<head>\n<title>RequiredModInstaller</title>\n</head>\n<body>\n<h1>RequiredModInstaller Sources</h1>\n<a href = {'"'}{'"'} target = {'"'}_self{'"'}><h2>Verified Mods</h2></a>\n";
            for (int i = 0; i <= names.Length; i++)
            {
                websiteSource += $"<a href = {'"'}{source[i]}{'"'} target = {'"'}_self{'"'}>{names[i]}</a>\n";
            }
            websiteSource += "</body>\n</html>";

            File.WriteAllText(exportPath, websiteSource);
        }

    }
}
