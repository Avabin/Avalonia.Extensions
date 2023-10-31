using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ReactiveUI;

namespace Avalonia.Extensions.Hosting;

public class AppViewLocator : IDataTemplate, IViewLocator
{
    public Control Build(object? data)
    {
        if (data == null) return new TextBlock { Text = "Null" };
        // create IViewFor<T> for data
        var dataType = data.GetType();
        var viewType = typeof(IViewFor<>).MakeGenericType(dataType);

        // get the view
        var view = (IViewFor?)ServiceLocator.Get(viewType);

        if (view == null)
        {
            // fallback to Activator
            try
            {
                view = (IViewFor?)Activator.CreateInstance(viewType);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new TextBlock { Text = e.Message };
            }
            if (view == null)
            {
                return new TextBlock { Text = "Null" };
            }
            view.ViewModel = data;
            return (Control)view;
        }

        view.ViewModel = data;
        return (Control)view;
    }

    public bool Match(object? data)
    {
        return data is ReactiveObject;
    }

    public IViewFor? ResolveView<T>(T? viewModel, string? contract = null)
    {
        if (viewModel == null) return null;
        var viewTypeGeneric = typeof(IViewFor<>);
        var viewModelType = viewModel.GetType();
        var viewType = viewTypeGeneric.MakeGenericType(viewModelType);
        
        var view = (IViewFor?)ServiceLocator.Get(viewType);
        if (view == null)
        {
            // fallback to Activator
            try
            {
                view = (IViewFor?)Activator.CreateInstance(viewType);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
            if (view == null)
            {
                return null;
            }
            view.ViewModel = viewModel;
            return view;
        }

        view.ViewModel = viewModel;
        return view;
    }
}