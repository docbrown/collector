Sub Main()
    Calendar = [
        { Date: "2023-05-08T00:00:00", Title: "Truman Day (CLOSED)" },
        { Date: "2023-05-29T00:00:00", Title: "Memorial Day (CLOSED)" },
        { Date: "2023-06-19T00:00:00", Title: "Juneteenth (CLOSED)" },
        { Date: "2023-07-04T00:00:00", Title: "Independence Day (CLOSED)" },
        { Date: "2023-09-04T00:00:00", Title: "Labor Day (CLOSED)" },
        { Date: "2023-08-28T10:00:00", Title: "Tax Sale" },
        { Date: "2023-10-09T00:00:00", Title: "Columbus Day (CLOSED)" },
        { Date: "2023-11-10T00:00:00", Title: "Veteran's Day (CLOSED)" },
        { Date: "2023-11-23T00:00:00", Title: "Thanksgiving (CLOSED)" },
        { Date: "2023-11-24T00:00:00", Title: "Thanksgiving (CLOSED)" },
        { Date: "2023-12-25T00:00:00", Title: "Christmas (CLOSED)" }
    ]

    ' Create fonts
    FontRegistry = CreateObject("roFontRegistry")
    OfficeNameFont = FontRegistry.GetDefaultFont(40, True, False)
    OfficeDescriptionFont = FontRegistry.GetDefaultFont(24, False, False)
    TimeFont = FontRegistry.GetDefaultFont(40, True, False)
    DateFont = FontRegistry.GetDefaultFont(24, False, False)
    EventDateFont = FontRegistry.GetDefaultFont(22, True, False)
    EventTitleFont = FontRegistry.GetDefaultFont(24, False, False)

    ' Load audio resources
    AlwaysOnEnabledSound = CreateObject("roAudioResource", "pkg:/audio/always-on-enabled.wav")
    AlwaysOnDisabledSound = CreateObject("roAudioResource", "pkg:/audio/always-on-disabled.wav")

    ' Load image resources
    Background = CreateObject("roBitmap", "pkg:/images/background.png")
    OfficeArrow = CreateObject("roBitmap", "pkg:/images/office-arrow-right.png")
    ClosedSign = CreateObject("roBitmap", "pkg:/images/closed.png")

    ' Load slide images
    FileSystem = CreateObject("roFileSystem")
    m.Slides = []
    For Each Path In FileSystem.GetDirectoryListing("pkg:/images/slides")
        Slide = {
            Name: Path,
            Bitmap: CreateObject("roBitmap", "pkg:/images/slides/" + Path)
        }
        m.Slides.Push(Slide)
    Next
    m.Slides.SortBy("Name")

    ' The index of the currently visible slide in m.Slides
    m.SlideIndex = 0

    ' Tracks the elapsed time for the current slide
    SlideTimer = CreateObject("roTimeSpan")

    ' This holds the current value of the real-time clock. It is initially
    ' set to an arbitrary point in the past, which will be loaded into the
    ' PreviousClock variable on the first iteration of the main loop, causing
    ' time-dependent state initialization to occur.
    Clock = CreateObject("roDateTime")
    Clock.FromISO8601String("2000-01-01T00:00:00")

    ' This holds the value of the real-time clock in the previous iteration
    ' of the main loop and is used to detect time changes.
    PreviousClock = CreateObject("roDateTime")

    m.Port = CreateObject("roMessagePort")

    m.Screen = CreateObject("roScreen")
    m.Screen.SetMessagePort(m.Port)
    m.Screen.SetAlphaEnable(True)

    OfficeName = "Collector of Revenue"
    OfficeDescription = "Tax payments, statements, receipts, merchant licenses, tax sale"

    ' Screen element sizes and positions are hardcoded for a 720p
    ' resolution to give the main frame an aspect ratio of 1.5:1 (3:2).
    FrameWidth = 940
    FrameHeight = 626
    FrameX = 0
    FrameY = 0
    SideWidth = 340
    SideHeight = 626
    SideX = 940
    SideY = 0
    BottomWidth = 1280
    BottomHeight = 94
    BottomX = 0
    BottomY = 626

    OfficeNameWidth = OfficeNameFont.GetOneLineWidth(OfficeName, BottomWidth)
    OfficeNameHeight = OfficeNameFont.GetOneLineHeight()
    OfficeDescriptionWidth = OfficeDescriptionFont.GetOneLineWidth(OfficeDescription, BottomWidth)
    OfficeDescriptionHeight = OfficeDescriptionFont.GetOneLineHeight()
    OfficeNameX = BottomWidth - OfficeNameWidth - OfficeArrow.GetWidth() -20
    OfficeNameY = BottomY + (BottomHeight / 2) - ((OfficeNameHeight + OfficeDescriptionHeight) / 2)
    OfficeDescriptionX = BottomWidth - OfficeDescriptionWidth - OfficeArrow.GetWidth() - 20
    OfficeDescriptionY = OfficeNameY + OfficeNameHeight
    OfficeArrowX = BottomWidth - OfficeArrow.GetWidth() - 10
    OfficeArrowY = BottomY + (BottomHeight / 2) - (OfficeArrow.GetHeight() / 2)

    TimeHeight = TimeFont.GetOneLineHeight()
    DateHeight = DateFont.GetOneLineHeight()
    TimeX = 10
    TimeY = BottomY + (BottomHeight / 2) - ((TimeHeight + DateHeight) / 2)
    DateX = 10
    DateY = TimeY + TimeHeight

    CalendarX = FrameWidth + 20
    CalendarY = 20
    CalendarWidth = m.Screen.GetWidth() - CalendarX

    ' Perform one-time processing of the calendar
    Calendar.SortBy("Date")
    For Each Event In Calendar
        ' Parse the date and time
        Date = CreateObject("roDateTime")
        Date.FromISO8601String(Event.Date)
        Event.Date = Date

        ' Format the date and time for display
        DateString = Date.AsDateString("short-month-short-weekday")
        ' Strip off the year
        DateString = DateString.Left(DateString.Len()-6)
        ' Add the time if it is not midnight (midnight here means "all day")
        If Date.GetHours() <> 0
            DateString += " @ " + FormatTime(Date.GetHours(), Date.GetMinutes())
        End If
        Event.DateString = DateString

        ' Wrap title
        Event.Title = WrapText(Event.Title, CalendarWidth-20, EventTitleFont)
    Next

    ' Formatted date and time strings
    Date = ""
    Time = ""

    ' This holds the events visible on-screen
    Events = []

    ' If True, redraw the entire screen
    Redraw = True

    ' If True, the TV will not display the Closed for Lunch image or go blank during
    ' after hours. This is toggled by pressing the Up key on the remote. It is
    ' automatically set to False again at the start of each day.
    AlwaysOn = False

    ' If True, the Closed for Lunch image will be displayed instead of the slideshow.
    Lunch = False

    ' If True, the screen will be blank.
    AfterHours = False

    While(True)
        Msg = Wait(500, m.Port)

        ' Process button presses on the remote
        If Type(Msg) = "roUniversalControlEvent"
            Button = Msg.GetInt()
            If Button = 2 ' Up
                If AlwaysOn
                    AlwaysOn = False
                    AlwaysOnDisabledSound.Trigger(75)
                Else
                    AlwaysOn = True
                    AlwaysOnEnabledSound.Trigger(75)
                End If
                Redraw = True
            Else If Button = 4 ' Left
                PreviousSlide()
                SlideTimer.Mark()
                Redraw = True
            Else If Button = 5 ' Right
                NextSlide()
                SlideTimer.Mark()
                Redraw = True
            Else If Button = 7 ' Instant Replay
                RestartApp()
            End If
        End If

        ' Update the real-time clock
        PreviousClock.FromSeconds(Clock.AsSeconds())
        Clock.Mark()
        Clock.ToLocalTime()

        Hours = Clock.GetHours()
        DayOfWeek = Clock.GetDayOfWeek()

        ' Perform daily state changes
        If Clock.GetDayOfMonth() <> PreviousClock.GetDayOfMonth()
            ' Disable always-on mode, in case the user forgot to do it.
            AlwaysOn = False

            ' Rebuild the on-screen event list
            Events.Clear()
            X = CalendarX
            Y = CalendarY
            MaxY = FrameHeight

            For Each Event In Calendar
                ' Skip events in the past. We don't consider minutes or seconds
                ' in this check, because we want the current day's events to
                ' appear for the whole day.
                If Event.Date.GetMonth() < Clock.GetMonth() And Event.Date.GetDayOfMonth() < Clock.GetDayOfMonth()
                    Continue For
                End If
                
                ' Create a copy of the event with layout information
                NewEvent = {
                    DateString: Event.DateString,
                    Title: [],
                    X: X,
                    Y: Y,
                    Width: CalendarWidth,
                    Height: 20 + EventDateFont.GetOneLineHeight() + (EventTitleFont.GetOneLineHeight() * Event.Title.Count())
                }

                ' Stop if we can't fit anymore events on the screen
                If NewEvent.Y + NewEvent.Height > MaxY Then Exit For

                ' Calculate text positions
                NewEvent.DateX = NewEvent.X + 10
                NewEvent.DateY = NewEvent.Y + 10
                LineY = NewEvent.DateY + EventDateFont.GetOneLineHeight()
                For Each Line In Event.Title
                    NewEvent.Title.Push({
                        Text: Line,
                        X: NewEvent.X + 10,
                        Y: LineY
                    })
                    LineY += EventTitleFont.GetOneLineHeight()
                Next

                ' Add the event to the on-screen event list
                Events.Push(NewEvent)

                ' Advance Y position for the next event
                Y += NewEvent.Height + 10
            Next
        End If

        ' Perform hourly state changes
        If Clock.GetHours() <> PreviousClock.GetHours()
            KeepAwake()
            Lunch = (Hours = 12)
            AfterHours = (DayOfWeek = 0 Or DayOfWeek = 6 Or Hours > 16 Or Hours < 8)
            Redraw = True
        End If

        ' Update the on-screen clock every minute
        If Clock.GetMinutes() <> PreviousClock.GetMinutes()
            Time = FormatTime(Clock.GetHours(), Clock.GetMinutes())
            Date = Clock.AsDateString("long-date")
            Redraw = True
        End If

        ' Change slides every 15 seconds
        If SlideTimer.TotalSeconds() >= 15
            SlideTimer.Mark()
            NextSlide()
            Redraw = True
        End If

        ' Only redraw the screen if something has changed
        If Not Redraw
            Continue While
        End If
        Redraw = False

        m.Screen.Clear(&h000000FF)

        ' In after hours mode, the screen is simply blank
        If Not AlwaysOn And AfterHours
            m.Screen.SwapBuffers()
            Continue While
        End If

        m.Screen.DrawObject(0, 0, Background)

        DrawTextWithShadow(m.Screen, OfficeName, OfficeNameX, OfficeNameY, &hFFFFFFFF, &h0000007d, 2, 2, OfficeNameFont)
        DrawTextWithShadow(m.Screen, OfficeDescription, OfficeDescriptionX, OfficeDescriptionY, &hFFFFFFFF, &h0000007d, 2, 2, OfficeDescriptionFont)

        m.Screen.DrawObject(OfficeArrowX + 2, OfficeArrowY + 2, OfficeArrow, &h0000007d)
        m.Screen.DrawObject(OfficeArrowX, OfficeArrowY, OfficeArrow)

        DrawTextWithShadow(m.Screen, Time, TimeX, TimeY, &hFFFFFFFF, &h0000007d, 2, 2, TimeFont)
        DrawTextWithShadow(m.Screen, Date, DateX, DateY, &hFFFFFFFF, &h0000007d, 2, 2, DateFont)

        For Each Event In Events
            m.Screen.DrawRect(Event.X, Event.Y, Event.Width, Event.Height, &h0000007d)
            DrawTextWithShadow(m.Screen, Event.DateString, Event.DateX, Event.DateY, &hFFFFFFFF, &h0000007d, 2, 2, EventDateFont)
            For Each Line in Event.Title
                DrawTextWithShadow(m.Screen, Line.Text, Line.X, Line.Y, &hFFFFFFFF, &h0000007d, 2, 2, EventTitleFont)
            Next
        Next

        If Not AlwaysOn And Lunch
            m.Screen.DrawObject(0, 0, ClosedSign)
        Else
            m.Screen.DrawObject(0, 0, m.Slides[m.SlideIndex].Bitmap)
        End If

        ' Draw a little green dot if always-on mode is enabled
        If AlwaysOn
            m.Screen.DrawPoint(10, 10, 5, &h00FF00FF)
        End If

        m.Screen.SwapBuffers()
    End While
End Sub

Sub DrawTextWithShadow(Dest As Object, Text As String, X As Integer, Y As Integer, TextColor As Integer, ShadowColor As Integer, ShadowOffsetX As Integer, ShadowOffsetY As Integer, Font As Object)
    Dest.DrawText(Text, X + ShadowOffsetX, Y + ShadowOffsetY, ShadowColor, Font)
    Dest.DrawText(Text, X, Y, TextColor, Font)
End Sub

Function FormatTime(Hours As Integer, Minutes As Integer) As String
    PM = False
    If Hours > 12
        PM = True
        Hours -= 12
    End If
    Time = Str(Hours).Trim()
    Time += ":"
    If Minutes < 10
        Time += "0"
    End If
    Time += Str(Minutes).Trim()
    If PM
        Time += " PM"
    Else
        Time += " AM"
    End If
    Return Time
End Function

Sub RestartApp()
    ' Send an ECP request to restart ourselves
    AppId = CreateObject("roAppInfo").GetID()
    Url = Substitute("http://localhost:8060/launch/{0}?restart=true", AppId)
    Request = CreateObject("roUrlTransfer")
    Request.SetUrl(Url)
    Request.PostFromString("")
    ' Wait for the app to restart
    While (True)
        Sleep(1000)
    End While
End Sub

Sub KeepAwake()
    ' Send an ECP request to simulate a button press so that the app
    ' doesn't exit after 2 hours.
    Request = CreateObject("roUrlTransfer")
    Request.SetUrl("http://localhost:8060/keypress/Select")
    Request.PostFromString("")
End Sub

Function WrapText(Text As String, Width As Integer, Font As Object) As Object
    EndWord = Function(Text As String, Index As Integer) As Integer
        For P = Index To Text.Len()
            If Text.Mid(P, 1) <> " " Then Exit For
        End For
        For P = P To Text.Len()
            If Text.Mid(P ,1) = " " Then Exit For
        End For
        Return P
    End Function
    Lines = []
    Line = 0
    While True
        While Text.Mid(Line, 1) = " "
            Line = Line + 1
        End While
        EndLine = Line
        While True
            If EndLine >= Text.Len() Then
                If EndLine > Line Then Lines.Push(Text.Mid(Line, EndLine-Line))
                Return Lines
            End If
            E = EndWord(Text, EndLine)
            If Font.GetOneLineWidth(Text.Mid(Line, E-Line), Width) >= Width then
                Lines.Push(Text.Mid(Line, EndLine-Line))
                Line = EndLine
                Exit While
            End If
            EndLine = E
        End While
    End While
End Function

Sub NextSlide()
    m.SlideIndex++
    If m.SlideIndex = m.Slides.Count()
        m.SlideIndex = 0
    End If
End Sub

Sub PreviousSlide()
    m.SlideIndex--
    If m.SlideIndex < 0
        m.SlideIndex = m.Slides.Count() - 1
    End If
End Sub
