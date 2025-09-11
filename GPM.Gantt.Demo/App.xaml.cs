﻿using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace GPM.Gantt.Demo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Debug.WriteLine("App.OnStartup: Starting");
        try
        {
            base.OnStartup(e);
            Debug.WriteLine("App.OnStartup: Base startup completed");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"App.OnStartup: Exception occurred: {ex}");
            throw;
        }
    }
}