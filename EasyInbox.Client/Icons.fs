module Icons

open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Controls.Shapes


let MailIcon = 
        Canvas.create [
            Canvas.width 24.0
            Canvas.height 24.0
            Canvas.children [
                Path.create [
                    Path.fill "white"
                    Path.data "M22 6C22 4.9 21.1 4 20 4H4C2.9 4 2 4.9 2 6V18C2 19.1 2.9 20 4 20H20C21.1 20 22 19.1 22 18V6M20 6L12 11L4 6H20M20 18H4V8L12 13L20 8V18Z"
                ]
            ]
        ]

let FileMultiple = 
    Canvas.create [
        Canvas.width 24.0
        Canvas.height 24.0
        Canvas.children [
            Path.create [
                Path.fill "white"
                Path.data "M16 0H8C6.9 0 6 .9 6 2V18C6 19.1 6.9 20 8 20H20C21.1 20 22 19.1 22 18V6L16 0M20 18H8V2H15V7H20V18M4 4V22H20V24H4C2.9 24 2 23.1 2 22V4H4Z"
            ]
        ]
    ]
    

