using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.Interface.Pages
{
    public partial class Index
    {
        const string _iconTemperature = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 64 64\"><defs><style>.cls-1{fill:#e1251b;}</style></defs><g id=\"_42.thermometer\" data-name=\"42.thermometer\"><path d=\"M36.11,39.44V10.74a6.71,6.71,0,0,0-13.41,0v28.7C13.88,45.73,18.56,60.06,29.41,60,40.25,60.06,44.94,45.73,36.11,39.44ZM29.41,58c-9.13.16-12.86-12.31-5.15-17.19a1.15,1.15,0,0,0,.44-.84V10.74a4.71,4.71,0,0,1,9.41,0V39.93a1.22,1.22,0,0,0,.45.84C42.26,45.64,38.53,58.12,29.41,58Z\"/><path class=\"cls-1\" d=\"M30.89,42.56c.05-.15,0-30.1,0-30.26a1.5,1.5,0,0,0-3,0c0,.17,0,30.11,0,30.26-7,1.71-5.82,12.22,1.48,12.25S37.9,44.27,30.89,42.56Z\"/><path class=\"cls-1\" d=\"M45,34.63h-5.7a1,1,0,0,0,0,2H45A1,1,0,0,0,45,34.63Z\"/><path class=\"cls-1\" d=\"M45,30.48h-5.7a1,1,0,0,0,0,2H45A1,1,0,0,0,45,30.48Z\"/><path class=\"cls-1\" d=\"M45,26.33h-5.7a1,1,0,0,0,0,2H45A1,1,0,0,0,45,26.33Z\"/><path class=\"cls-1\" d=\"M45,22.19h-5.7a1,1,0,0,0,0,2H45A1,1,0,0,0,45,22.19Z\"/><path class=\"cls-1\" d=\"M45,18h-5.7a1,1,0,0,0,0,2H45A1,1,0,0,0,45,18Z\"/><path class=\"cls-1\" d=\"M39.26,15.89H45a1,1,0,0,0,0-2h-5.7A1,1,0,0,0,39.26,15.89Z\"/></g></svg>";
        const string _iconHumidity = @"<svg style=""width:24px;height:24px"" viewBox=""0 0 24 24"">
<path fill=""currentColor"" d=""M12,3.25C12,3.25 6,10 6,14C6,17.32 8.69,20 12,20A6,6 0 0,0 18,14C18,10 12,3.25 12,3.25M14.47,9.97L15.53,11.03L9.53,17.03L8.47,15.97M9.75,10A1.25,1.25 0 0,1 11,11.25A1.25,1.25 0 0,1 9.75,12.5A1.25,1.25 0 0,1 8.5,11.25A1.25,1.25 0 0,1 9.75,10M14.25,14.5A1.25,1.25 0 0,1 15.5,15.75A1.25,1.25 0 0,1 14.25,17A1.25,1.25 0 0,1 13,15.75A1.25,1.25 0 0,1 14.25,14.5Z"" />";
        const string _iconFrequency = @"<svg style=""width:24px;height:24px"" viewBox=""0 0 24 24"">
<path fill=""currentColor"" d=""M16.5,21C13.5,21 12.31,16.76 11.05,12.28C10.14,9.04 9,5 7.5,5C4.11,5 4,11.93 4,12H2C2,11.63 2.06,3 7.5,3C10.5,3 11.71,7.25 12.97,11.74C13.83,14.8 15,19 16.5,19C19.94,19 20.03,12.07 20.03,12H22.03C22.03,12.37 21.97,21 16.5,21Z"" />";
        const string _iconMonitor = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?> <svg id=\"Layer_1\" data-name=\"Layer 1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" viewBox=\"0 0 512 512\"><defs><style>.cls-1{fill:url(#linear-gradient);}.cls-2{fill:url(#linear-gradient-2);}.cls-3{fill:url(#linear-gradient-3);}.cls-4{fill:url(#linear-gradient-4);}.cls-5{fill:url(#linear-gradient-5);}</style><linearGradient id=\"linear-gradient\" x1=\"210.638\" y1=\"346.174\" x2=\"295.669\" y2=\"427.077\" gradientUnits=\"userSpaceOnUse\"><stop offset=\"0\" stop-color=\"#3e4154\"/><stop offset=\"1\" stop-color=\"#1b2129\"/></linearGradient><linearGradient id=\"linear-gradient-2\" x1=\"34.105\" y1=\"67.412\" x2=\"474.945\" y2=\"367.91\" xlink:href=\"#linear-gradient\"/><linearGradient id=\"linear-gradient-3\" x1=\"45.564\" y1=\"89.707\" x2=\"464.491\" y2=\"325.199\" gradientUnits=\"userSpaceOnUse\"><stop offset=\"0\" stop-color=\"#6fc6fc\"/><stop offset=\"1\" stop-color=\"#50a7f6\"/></linearGradient><linearGradient id=\"linear-gradient-4\" x1=\"256\" y1=\"406.614\" x2=\"256\" y2=\"444.733\" xlink:href=\"#linear-gradient\"/><linearGradient id=\"linear-gradient-5\" x1=\"242.381\" y1=\"338.381\" x2=\"266.738\" y2=\"362.738\" gradientUnits=\"userSpaceOnUse\"><stop offset=\"0\" stop-color=\"#e3f0ff\"/><stop offset=\"1\" stop-color=\"#90d5fe\"/></linearGradient></defs><rect class=\"cls-1\" x=\"213.333\" y=\"341.333\" width=\"85.333\" height=\"96\"/><rect class=\"cls-2\" x=\"21.333\" y=\"64\" width=\"469.333\" height=\"309.333\" rx=\"32\"/><rect class=\"cls-3\" x=\"42.667\" y=\"85.333\" width=\"426.667\" height=\"245.333\" rx=\"16\"/><path class=\"cls-4\" d=\"M421.333,416H90.667A26.667,26.667,0,0,0,64,442.667h0A5.333,5.333,0,0,0,69.333,448H442.667A5.333,5.333,0,0,0,448,442.667h0A26.667,26.667,0,0,0,421.333,416Z\"/><circle class=\"cls-5\" cx=\"256\" cy=\"352\" r=\"10.667\"/></svg>";
        const string _iconFan = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?> <svg id=\"Layer_1\" data-name=\"Layer 1\" xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 512 512\"><defs><style>.cls-1{fill:#2d4a60;}.cls-2{fill:#eaeff0;}.cls-3{fill:#8fa5a5;}</style></defs><title>Fan</title><rect class=\"cls-1\" x=\"53.333\" y=\"53.333\" width=\"405.333\" height=\"405.333\" rx=\"21.333\" ry=\"21.333\"/><circle class=\"cls-2\" cx=\"256\" cy=\"256\" r=\"160\"/><path class=\"cls-1\" d=\"M261.521,276.606a21.169,21.169,0,0,1-13.039-.695,84.219,84.219,0,0,0-6.429,96.588,116.956,116.956,0,0,0,44.315-3.164,118.33,118.33,0,0,0,13.24-4.43,107.764,107.764,0,0,1-29.745-92.783A21.125,21.125,0,0,1,261.521,276.606Z\"/><path class=\"cls-1\" d=\"M212.392,147.095a107.766,107.766,0,0,1,29.746,92.783,20.892,20.892,0,0,1,21.38-3.79,84.219,84.219,0,0,0,6.429-96.587,116.956,116.956,0,0,0-44.315,3.164A118.33,118.33,0,0,0,212.392,147.095Z\"/><path class=\"cls-1\" d=\"M276.606,261.521a21.169,21.169,0,0,1-7.121,10.945,84.219,84.219,0,0,0,80.433,53.861,116.956,116.956,0,0,0,19.417-39.96,118.331,118.331,0,0,0,2.784-13.681,107.764,107.764,0,0,1-95.225-20.632A21.125,21.125,0,0,1,276.606,261.521Z\"/><path class=\"cls-1\" d=\"M139.881,239.313a107.766,107.766,0,0,1,95.225,20.631,20.892,20.892,0,0,1,7.408-20.411,84.219,84.219,0,0,0-80.433-53.861,116.956,116.956,0,0,0-19.417,39.96A118.331,118.331,0,0,0,139.881,239.313Z\"/><path class=\"cls-1\" d=\"M271.085,240.915A21.169,21.169,0,0,1,277,252.555a84.219,84.219,0,0,0,86.862-42.727,116.956,116.956,0,0,0-24.9-36.8,118.331,118.331,0,0,0-10.457-9.251,107.764,107.764,0,0,1-65.48,72.152A21.125,21.125,0,0,1,271.085,240.915Z\"/><path class=\"cls-1\" d=\"M183.489,348.219a107.766,107.766,0,0,1,65.48-72.152A20.892,20.892,0,0,1,235,259.445a84.219,84.219,0,0,0-86.862,42.726,116.956,116.956,0,0,0,24.9,36.8A118.331,118.331,0,0,0,183.489,348.219Z\"/><circle class=\"cls-3\" cx=\"256\" cy=\"256\" r=\"21.333\"/><path d=\"M437.333,42.667H74.667a32.035,32.035,0,0,0-32,32V437.333a32.035,32.035,0,0,0,32,32H437.333a32.035,32.035,0,0,0,32-32V74.667A32.035,32.035,0,0,0,437.333,42.667ZM448,437.333A10.681,10.681,0,0,1,437.333,448H74.667A10.681,10.681,0,0,1,64,437.333V74.667A10.679,10.679,0,0,1,74.667,64H437.333A10.679,10.679,0,0,1,448,74.667Z\"/><path d=\"M256,85.333c-94.1,0-170.667,76.56-170.667,170.667S161.9,426.667,256,426.667,426.667,350.1,426.667,256,350.1,85.333,256,85.333Zm0,320c-82.344,0-149.333-66.99-149.333-149.333S173.656,106.667,256,106.667,405.333,173.656,405.333,256,338.344,405.333,256,405.333Z\"/><circle cx=\"96\" cy=\"96\" r=\"10.667\"/><circle cx=\"416\" cy=\"96\" r=\"10.667\"/><circle cx=\"96\" cy=\"416\" r=\"10.667\"/><circle cx=\"416\" cy=\"416\" r=\"10.667\"/><path d=\"M379.245,264.75a10.712,10.712,0,0,0-9.781-2.4,97.055,97.055,0,0,1-44.323,1.146,95.077,95.077,0,0,1-10-2.711,95.091,95.091,0,0,0,17.279-6.56,96.411,96.411,0,0,0,40.781-39.25,10.658,10.658,0,0,0,.469-9.344,127.254,127.254,0,0,0-38.557-50.229,10.668,10.668,0,0,0-16.88,5.508,97.089,97.089,0,0,1-21.167,38.956,94.824,94.824,0,0,1-7.363,7.318,95.31,95.31,0,0,0,2.972-18.261,96.507,96.507,0,0,0-13.594-54.937,10.67,10.67,0,0,0-7.865-5.081,127.385,127.385,0,0,0-48.349,3.453,129.434,129.434,0,0,0-14.432,4.826,10.666,10.666,0,0,0-3.667,17.372,97.136,97.136,0,0,1,23.156,37.807,94.946,94.946,0,0,1,2.668,10.1,93.681,93.681,0,0,0-68.72-27.459,10.677,10.677,0,0,0-8.328,4.266,127.419,127.419,0,0,0-24.224,58.508,10.671,10.671,0,0,0,13.214,11.862,97.236,97.236,0,0,1,44.323-1.146,95.009,95.009,0,0,1,10.086,2.74A93.64,93.64,0,0,0,138.8,297.021a10.657,10.657,0,0,0-.469,9.344,127.315,127.315,0,0,0,27.167,40.151,129.037,129.037,0,0,0,11.391,10.078,10.665,10.665,0,0,0,16.88-5.51,97.079,97.079,0,0,1,21.167-38.953,94.977,94.977,0,0,1,7.363-7.32,95.277,95.277,0,0,0-2.973,18.257,96.5,96.5,0,0,0,13.594,54.943,10.667,10.667,0,0,0,7.865,5.078,127.5,127.5,0,0,0,62.786-8.276,10.668,10.668,0,0,0,3.661-17.37,97.136,97.136,0,0,1-23.156-37.812,95.125,95.125,0,0,1-2.667-10.092,93.666,93.666,0,0,0,67.005,27.472c.6,0,1.172-.005,1.714-.016a10.682,10.682,0,0,0,8.328-4.271,127.374,127.374,0,0,0,24.224-58.505A10.672,10.672,0,0,0,379.245,264.75ZM199.016,297.922a117.388,117.388,0,0,0-20.234,31.667,105.931,105.931,0,0,1-18.615-26.745,73.439,73.439,0,0,1,66.74-33.568,31.172,31.172,0,0,0,2.375,4.328A118.759,118.759,0,0,0,199.016,297.922Zm28.38-56.266A119.2,119.2,0,0,0,191.2,227.612a117.33,117.33,0,0,0-37.542-1.69,105.967,105.967,0,0,1,13.854-29.492,73.446,73.446,0,0,1,62.443,41.01A31.923,31.923,0,0,0,227.4,241.656Zm3.484-89.323a105.828,105.828,0,0,1,32.464-2.747,73.391,73.391,0,0,1-4.292,74.578c-1.047-.1-2.1-.156-3.146-.156-.6,0-1.193.018-1.792.052a118.77,118.77,0,0,0-5.927-38.372A117.362,117.362,0,0,0,230.88,152.333ZM266.667,256A10.667,10.667,0,1,1,256,245.333,10.667,10.667,0,0,1,266.667,256Zm-18.01,106.417a73.4,73.4,0,0,1,4.292-74.583,31.235,31.235,0,0,0,4.938.1,118.725,118.725,0,0,0,5.927,38.37,117.357,117.357,0,0,0,17.307,33.359A105.723,105.723,0,0,1,248.656,362.417Zm64.328-148.339a117.348,117.348,0,0,0,20.234-31.667,106.064,106.064,0,0,1,18.615,26.745,73.439,73.439,0,0,1-66.74,33.568,31.115,31.115,0,0,0-2.375-4.323A119.136,119.136,0,0,0,312.984,214.078Zm31.5,101.495a73.446,73.446,0,0,1-62.443-41.01,31.137,31.137,0,0,0,2.563-4.224A118.8,118.8,0,0,0,320.8,284.385a117.167,117.167,0,0,0,37.542,1.693A106.017,106.017,0,0,1,344.484,315.573Z\"/></svg>";
        const string _iconCooler = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<!-- Generator: Adobe Illustrator 23.0.3, SVG Export Plug-In . SVG Version: 6.00 Build 0)  -->\r\n<svg version=\"1.1\" id=\"Layer_1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" x=\"0px\" y=\"0px\"\r\n\t viewBox=\"0 0 100 100\" enable-background=\"new 0 0 100 100\" xml:space=\"preserve\">\r\n<path d=\"M5.33,15.6v64.26c0,4.77,3.88,8.64,8.64,8.64h3.36l0.21,3.91c0.04,0.8,0.7,1.42,1.5,1.42h10.11c0.8,0,1.45-0.62,1.5-1.42\r\n\tl0.21-3.91h38.44l0.31,3.95c0.06,0.78,0.71,1.38,1.5,1.38h9.56c0.78,0,1.43-0.6,1.5-1.38l0.31-3.95h3.54c4.77,0,8.64-3.88,8.64-8.64\r\n\tV15.6c0-0.01,0-0.02,0-0.03s0-0.02,0-0.03v-0.73c0-4.77-3.88-8.64-8.64-8.64H13.98c-4.77,0-8.64,3.88-8.64,8.64v0.73\r\n\tc0,0.01,0,0.02,0,0.03S5.33,15.59,5.33,15.6z M27.74,90.84h-7.27l-0.13-2.33h7.52L27.74,90.84z M79.29,90.84h-6.79l-0.18-2.33h7.15\r\n\tL79.29,90.84z M86.02,85.51H13.98c-3.11,0-5.64-2.53-5.64-5.64V17.07h83.34v62.79C91.67,82.98,89.14,85.51,86.02,85.51z M13.98,9.16\r\n\th72.05c2.86,0,5.23,2.14,5.59,4.91H8.39C8.75,11.31,11.11,9.16,13.98,9.16z\"/>\r\n<path d=\"M75.83,49.89l-5.02,2.78l-12.65-3.64l12.35-4.55l5.37,2.48c0.75,0.35,1.64,0.02,1.99-0.73c0.09-0.2,0.14-0.42,0.14-0.63\r\n\tc0-0.57-0.32-1.11-0.87-1.36l-4.57-2.11l2.04-4.42c0.35-0.75,0.02-1.64-0.73-1.99c-0.75-0.35-1.64-0.02-1.99,0.73l-2.4,5.21\r\n\tl-12.35,4.55l7.26-10.98l5.79-1.18c0.71-0.14,1.2-0.77,1.2-1.47c0-0.1-0.01-0.2-0.03-0.3c-0.17-0.81-0.96-1.33-1.77-1.17l-4.93,1.01\r\n\tl-0.97-4.77c-0.17-0.81-0.96-1.33-1.77-1.17c-0.81,0.17-1.34,0.96-1.17,1.77l1.15,5.63l-7.04,10.64V31.47l4.18-4.18\r\n\tc0.29-0.29,0.44-0.68,0.44-1.06c0-0.38-0.15-0.77-0.44-1.06c-0.59-0.59-1.54-0.59-2.12,0l-3.56,3.56l-3.44-3.44\r\n\tc-0.59-0.59-1.54-0.59-2.12,0c-0.59,0.59-0.59,1.54,0,2.12l4.06,4.06v13.16l-7.79-10.61l0.9-5.85c0.01-0.08,0.02-0.15,0.02-0.23\r\n\tc0-0.73-0.53-1.37-1.27-1.48c-0.82-0.13-1.58,0.44-1.71,1.25l-0.76,4.98l-4.81-0.74c-0.82-0.13-1.58,0.44-1.71,1.25\r\n\tc-0.13,0.82,0.44,1.58,1.25,1.71l5.67,0.87l7.79,10.61l-12.56-3.94l-2.74-5.24c-0.38-0.73-1.29-1.02-2.02-0.64\r\n\tc-0.51,0.27-0.81,0.79-0.81,1.33c0,0.23,0.05,0.47,0.17,0.69l2.33,4.46l-4.31,2.25c-0.73,0.38-1.02,1.29-0.64,2.02\r\n\tc0.38,0.74,1.29,1.02,2.02,0.64l5.09-2.66l12.56,3.95l-12.46,4.25l-5.31-2.61c-0.74-0.36-1.64-0.06-2.01,0.69\r\n\tc-0.1,0.21-0.15,0.44-0.15,0.66c0,0.55,0.31,1.09,0.84,1.35l4.52,2.22l-2.14,4.37c-0.37,0.74-0.06,1.64,0.69,2.01\r\n\tc0.21,0.1,0.44,0.15,0.66,0.15c0.55,0,1.09-0.31,1.35-0.84l2.53-5.15l12.46-4.25l-7.53,10.8l-5.82,1.04\r\n\tc-0.73,0.13-1.24,0.76-1.24,1.47c0,0.09,0.01,0.18,0.02,0.27c0.15,0.82,0.93,1.36,1.74,1.21l4.96-0.88l0.85,4.79\r\n\tc0.13,0.73,0.76,1.24,1.47,1.24c0.09,0,0.18-0.01,0.27-0.02c0.82-0.15,1.36-0.92,1.21-1.74l-1.01-5.65l7.53-10.8l0.32,13.16\r\n\tl-4.08,4.28c-0.28,0.29-0.41,0.66-0.41,1.03c0,0.4,0.16,0.79,0.47,1.09c0.29,0.28,0.66,0.41,1.03,0.41c0.4,0,0.79-0.16,1.09-0.47\r\n\tl3.47-3.65l3.52,3.35c0.6,0.57,1.55,0.55,2.12-0.05c0.57-0.6,0.55-1.55-0.05-2.12l-4.16-3.96l-0.32-13.16L63,64.17l-0.75,5.87\r\n\tc-0.01,0.07-0.01,0.13-0.01,0.19c0,0.74,0.55,1.39,1.31,1.49c0.07,0.01,0.13,0.01,0.19,0.01c0.74,0,1.39-0.55,1.49-1.31l0.64-4.99\r\n\tl4.82,0.62c0.83,0.11,1.57-0.48,1.68-1.3c0.11-0.82-0.47-1.57-1.3-1.68l-5.7-0.73l-8.05-10.42l12.65,3.64l2.86,5.17\r\n\tc0.27,0.49,0.79,0.77,1.31,0.77c0.25,0,0.5-0.06,0.73-0.19c0.49-0.27,0.77-0.79,0.77-1.31c0-0.25-0.06-0.49-0.19-0.72l-2.44-4.41\r\n\tl4.26-2.35c0.72-0.4,0.99-1.31,0.59-2.04C77.47,49.75,76.56,49.49,75.83,49.89z\"/>\r\n<path d=\"M21.25,30.92h-9.96c-0.83,0-1.5,0.67-1.5,1.5v33.59c0,0.83,0.67,1.5,1.5,1.5h9.96c0.83,0,1.5-0.67,1.5-1.5V32.42\r\n\tC22.75,31.59,22.08,30.92,21.25,30.92z M19.75,64.51h-6.96V33.92h6.96V64.51z\"/>\r\n</svg>";
        //hinh anh the hien trang thai quat dan mat
        const string _fanOff = "/images/CoolFanStop.jpg";
        const string _fanOn = "/images/CoolFanRun1.jpg";
        const string _coollerOff = "/images/DanMatOff.png";
        const string _coolerOn = "/images/DanMatOn.png";

        private MudTheme _theme = new MudTheme();

        //[Inject] public ISDisplayRealtime _displayRealtimeApiClient { get; set; }

        private List<APIClient.DisplayRealTimeModel> _displayRealtime;
        private APIClient.DisplayRealTimeModel _chuong1 = new APIClient.DisplayRealTimeModel();
        private APIClient.DisplayRealTimeModel _chuong2 = new APIClient.DisplayRealTimeModel();
        private APIClient.DisplayRealTimeModel _chuong3 = new APIClient.DisplayRealTimeModel();
        private APIClient.DisplayRealTimeModel _chuong4 = new APIClient.DisplayRealTimeModel();

        private List<APIClient.ChuongInfoModel> _chuongInfo;

        private System.Threading.Timer _timer;

        protected override async Task OnInitializedAsync()
        {
            var chuongInfoRes = await _chuongInfoClient.GetAllAsync();
            if (chuongInfoRes.Succeeded)
            {
                _chuongInfo = chuongInfoRes.Data.ToList();
            }
            else
            {
                foreach (var item in chuongInfoRes.Messages)
                {
                    _snackBar.Add(item, Severity.Error);
                }
            }

            var res = await _displayRealtimeClient.GetAllAsync();

            if (res.Succeeded)
            {
                _displayRealtime = res.Data.ToList();

                if (_displayRealtime == null && _displayRealtime.Count <= 0)
                {
                    _snackBar.Add("Data Null", Severity.Warning);
                    return;
                }

                foreach (var item in _chuongInfo)
                {
                    if (item.NumIndex == 1)
                    {
                        _chuong1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == item.Id);
                        _chuong1.TenChuong = item.TenChuong;
                    }
                    else if (item.NumIndex == 2)
                    {
                        _chuong2 = _displayRealtime.FirstOrDefault(x => x.ChuongId == item.Id);
                        _chuong2.TenChuong = item.TenChuong;
                    }
                    else if (item.NumIndex == 3)
                    {
                        _chuong3 = _displayRealtime.FirstOrDefault(x => x.ChuongId == item.Id);
                        _chuong3.TenChuong = item.TenChuong;
                    }
                    else// if (item.NumIndex == 4)
                    {
                        _chuong4 = _displayRealtime.FirstOrDefault(x => x.ChuongId == item.Id);
                        _chuong4.TenChuong = item.TenChuong;
                    }
                }
            }
            else
            {
                foreach (var item in res.Messages)
                {
                    _snackBar.Add(item, Severity.Error);
                }
            }

            #region Timer refresh data
            _timer = new System.Threading.Timer(async (object stateInfo) =>
            {
                //get thong tin chuong
                var chuongInfoRes = await _chuongInfoClient.GetAllAsync();
                if (chuongInfoRes.Succeeded)
                {
                    _chuongInfo = chuongInfoRes.Data.ToList();
                }

                //get thong tien hien thi thoi gian thuc
                var res = await _displayRealtimeClient.GetAllAsync();

                if (res.Succeeded)
                {
                    _displayRealtime = res.Data.ToList();

                    if (_displayRealtime == null && _displayRealtime.Count <= 0)
                    {
                        _snackBar.Add("Data Null", Severity.Warning);
                        return;
                    }

                    foreach (var item in _chuongInfo)
                    {
                        if (item.NumIndex == 1)
                        {
                            _chuong1 = _displayRealtime.FirstOrDefault(x => x.ChuongId == item.Id);
                            _chuong1.TenChuong = item.TenChuong;
                        }
                        else if (item.NumIndex == 2)
                        {
                            _chuong2 = _displayRealtime.FirstOrDefault(x => x.ChuongId == item.Id);
                            _chuong2.TenChuong = item.TenChuong;
                        }
                        else if (item.NumIndex == 3)
                        {
                            _chuong3 = _displayRealtime.FirstOrDefault(x => x.ChuongId == item.Id);
                            _chuong3.TenChuong = item.TenChuong;
                        }
                        else// if (item.NumIndex == 4)
                        {
                            _chuong4 = _displayRealtime.FirstOrDefault(x => x.ChuongId == item.Id);
                            _chuong4.TenChuong = item.TenChuong;
                        }
                    }
                }

                StateHasChanged(); // NOTE: MUST CALL StateHasChanged() BECAUSE THIS IS TRIGGERED BY A TIMER INSTEAD OF A USER EVENT
            }, new System.Threading.AutoResetEvent(false), 2000, 2000); // fire every 1000 milliseconds
            #endregion
        }
    }
}
