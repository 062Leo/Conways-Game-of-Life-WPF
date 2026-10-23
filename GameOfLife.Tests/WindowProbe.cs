using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace GameOfLife.Tests
{
    /// <summary>
    /// A MainWindow driven without being shown, so the handlers that only exist on
    /// the window can be put under test. The window is never a pure function, but
    /// state it fails to reset is exactly what a test has to hold.
    ///
    /// Everything runs on one shared STA thread: WPF objects belong to the thread
    /// that created them.
    /// </summary>
    internal sealed class WindowProbe
    {
        private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;

        private static readonly BlockingCollection<Action> Work = new BlockingCollection<Action>();
        private static readonly Lazy<Thread> UiThread = new Lazy<Thread>(StartUiThread);

        public MainWindow Window { get; }

        private WindowProbe()
        {
            Window = new MainWindow();
        }

        /// <summary>
        /// Runs the test body against a freshly built window that has been laid out
        /// at the given container size and has had its Loaded handler run.
        /// </summary>
        public static void Run(double containerWidth, double containerHeight, Action<WindowProbe> body)
        {
            _ = UiThread.Value;

            Exception? failure = null;
            using var done = new ManualResetEventSlim(false);

            Work.Add(() =>
            {
                try
                {
                    var probe = new WindowProbe();
                    probe.Layout(containerWidth, containerHeight);
                    probe.Call("MainWindow_Loaded", probe.Window, new RoutedEventArgs());
                    body(probe);
                }
                catch (Exception e)
                {
                    failure = e is TargetInvocationException invocation && invocation.InnerException != null
                        ? invocation.InnerException
                        : e;
                }
                finally
                {
                    done.Set();
                }
            });

            done.Wait();
            if (failure != null)
            {
                throw new InvalidOperationException("window probe failed: " + failure.Message, failure);
            }
        }

        /// <summary>
        /// Lays the canvas container out at the given size, which is what a window
        /// resize does, and runs the SizeChanged handler with it.
        /// </summary>
        public void Layout(double width, double height)
        {
            var container = Field<Border>("CanvasContainer");
            container.Measure(new Size(width, height));
            container.Arrange(new Rect(0, 0, width, height));
            container.UpdateLayout();
        }

        /// <summary>The cells currently alive on the window's grid.</summary>
        public HashSet<Point> LiveCells => Field<HashSet<Point>>("liveCells");

        /// <summary>
        /// The canvas pixel at the middle of a grid cell, which is what a mouse
        /// position over that cell looks like.
        /// </summary>
        public Point Pixel(int cellX, int cellY)
        {
            double size = Field<double>("cellSize");
            return new Point((cellX + 0.5) * size, (cellY + 0.5) * size);
        }

        public T Field<T>(string name) => (T)Window.GetType().GetField(name, Hidden)!.GetValue(Window)!;

        public void SetField(string name, object? value) => Window.GetType().GetField(name, Hidden)!.SetValue(Window, value);

        public object? Call(string name, params object?[] args)
            => Window.GetType().GetMethod(name, Hidden)!.Invoke(Window, args);

        private static Thread StartUiThread()
        {
            var thread = new Thread(() =>
            {
                if (Application.Current == null)
                {
                    _ = new Application();
                }

                foreach (var item in Work.GetConsumingEnumerable())
                {
                    item();
                }
            })
            {
                IsBackground = true
            };

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return thread;
        }
    }
}
