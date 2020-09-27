module ControlStyles
open Avalonia.Styling
open Styles
open Avalonia.Controls
open Avalonia.Controls.Presenters
open Avalonia.FuncUI.DSL
open Avalonia.Media

let listBoxItemSelected = 
    style 
        (fun s -> s.OfType<ListBoxItem>().Class(":selected").Template().OfType<ContentPresenter>())
        [ContentPresenter.background "#414244"]

let textBoxBorder = 
    style
        (fun s -> s.OfType<TextBox>().Template().OfType<Border>())
        [Border.cornerRadius 3.0]

let textBox = 
    style 
        (fun s -> s.OfType<TextBox>())
        [TextBox.borderThickness 1.8]

let textBoxFocused = 
    style 
        (fun s -> s.OfType<TextBox>().Class(":focus"))
        [TextBox.borderBrush(SolidColorBrush(Color.Parse("#000000"),0.6))]

let textBoxAll = [textBox; textBoxFocused; textBoxBorder]


let allStyles = [listBoxItemSelected; yield! textBoxAll]