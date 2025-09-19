using Markdig;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace AISupporter.App.Converters
{
    public class MarkdownToFlowDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string markdown && !string.IsNullOrEmpty(markdown))
            {
                try
                {
                    // Convert Markdown to FlowDocument
                    var pipeline = new MarkdownPipelineBuilder()
                        .UseAdvancedExtensions()
                        .Build();

                    var flowDocument = Markdig.Wpf.Markdown.ToFlowDocument(markdown, pipeline);

                    // Use FlowDocumentScrollViewer for clean display
                    var viewer = new FlowDocumentScrollViewer
                    {
                        Document = flowDocument,

                        // Clean appearance
                        BorderThickness = new Thickness(0),
                        Background = Brushes.Transparent,
                        Margin = new Thickness(0),
                        Padding = new Thickness(0),

                        // Auto height
                        Height = double.NaN,

                        // Remove scroll bars (parent handles scrolling)
                        VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,

                        // Remove focus and selection
                        Focusable = false,
                        IsTabStop = false,

                        // Remove toolbars
                        ContextMenu = null
                    };

                    if (flowDocument != null)
                    {
                        flowDocument.PagePadding = new Thickness(0);
                        flowDocument.LineHeight = 1.2;

                        // Reduce paragraph margins
                        foreach (Block block in flowDocument.Blocks)
                        {
                            if (block is Paragraph para)
                            {
                                para.Margin = new Thickness(0, 0, 0, 4);
                            }
                        }
                    }

                    return viewer;
                }
                catch
                {
                    // Fallback to simple TextBlock
                    return new TextBlock
                    {
                        Text = markdown,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 13
                    };
                }
            }

            return new TextBlock { Text = value?.ToString() ?? "", FontSize = 13 };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
