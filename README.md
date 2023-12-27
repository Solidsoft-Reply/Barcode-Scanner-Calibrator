# Barcode Scanner Calibration  
We often attach barcode scanners to computers as keyboard input devices.  In the past we often used keyboard wedges – e.g., ‘Y’ cables.  Today, we generally connect our barcode scanner via a USB port using the USB-HID (Human Interface Device) class.  Other options may exist.  In each case, the scanner acts as a keyboard device, reporting scan codes to the computer in accordance with a keyboard layout supported by the scanner.  The operating system then maps these codes to characters according to the current keyboard configuration.  
  
This is problematic.  Any incompatibility between the scanner’s internal keyboard layout and the operating system’s keyboard layout may lead to incorrect representation of the barcode data.  In addition, the scanner may be configured to change or invert the case of alphabetic characters, substitute certain characters for other characters, add prefixes and suffixes to the reported data or report control characters using one of several conventions.  Most barcode scanners support Microsoft Windows keyboard layouts and report control characters using Microsoft DOS conventions.  If the scanner does not support a given keyboard layout, or if it is used on other platforms such as macOS or Linux, it may be impossible to configure the barcode scanner to be compatible with the operating system.  
  
## Understanding the problem
To scan barcodes reliably, we need to align the behaviours of three main components.  These are:
* The barcode scanner
  
* The computer operating system  
  
* The application software which receives input from the barcode scanner.
  
In addition to incompatibilities between the barcode scanner and operating system, each application may introduce subtle problems and issues that may compromise the ability to scan barcodes reliably.
  
The conventional approach adapts the barcode scanner to the combination of the operating system and the application software.  We do this by configuring the barcode scanner.  Most scanners are configured by scanning special barcodes or sequences of barcodes provided by the manufacturer, or by running utility software that the manufacturer provides.  This is a problematic in situations where configuration must be done by non-technical computer users.  They may have to download manuals or software from the manufacturer’s web site and they may need a degree of technical knowledge that is not readily available to a non-technical user.  
  
Software providers often address this issue by providing a list of ‘approved’ (supported) barcode scanners (makes and models) for their software.  The software provider carefully tests their application against those barcode scanners and provides instructions to their end-users to help them configure their scanners to function correctly in given scenarios.  This approach, however, is not ideal.  It reduces the choice of barcode scanner to just a few models.  The list may need to change over time as older models are retired, and it may still pose issues to non-technical users. The software provider may also need to provide costly help-desk support to their users to help them with barcode scanning issues.  
## Calibration
There is an alternative approach.  Instead of adapting the barcode scanner to the combination of the operating systems and application software, we can adapt the application software to the combination of the barcode scanner and the operating system.  We call this approach ‘calibration’.  
  
In most cases, this can be done without having to change the configuration of the barcode scanner or the operating system, even if the barcode scanner is not configured with the same keyboard layout.  Calibration has several advantages:  
  
* End users do not need to download manuals or utility software from the manufacturer.
  
* End users can ‘calibrate’ their application software to the combination of the barcode scanner and operating system by scanning one, or sometimes a few, barcodes.  They do not need to acquire any additional technical knowledge or insight.
  
* The approach provides reliability without any need to maintain an approved list of supported barcode scanner models.  End users are therefore free to select from the widest range of barcode scanner models.
  
* The approach minimises or even eliminates the need for help-desk support for barcode scanner configuration issues.
  
* If the end user changes their barcode scanner or their computer configuration, they simply re-calibrate their application software.
  
## Why we wrote this library
The calibration library was developed to meet a specific need.  Across the European Union, and some additional countries, every pharmacist and dispensing doctor is required by law to scan a barcode on every pack of prescription medicine before supplying that medicine to a member of the public.  The scanned data is used to verify the medicine pack, helping to detect and eliminate any falsified medicines.  Medicines are dispensed from approximately 150,000 locations across the participating countries, and somewhere between 8 – 10 billion packs of prescription medicine are scanned every year.  
  
The European Medicines Verification System does not mandate any specific application software for verification.  There are over a thousand software providers and development teams who integrate their software with the system.  If there are any incompatibilities at the scanning stage, these may cause ‘false positive’ alerts which are sent to the relevant pharmaceutical manufacturer to warn them that the system may have detected a falsified medicine pack.  The manufacturer must then spend time and effort establishing why the alert has been raised and taking any appropriate action.  Clearly, the system requires a very high degree of reliability when scanning barcodes, but central control of the software, platforms or barcode scanners which are used to verify medicinal products is a matter for each member state, and many states exercise only lightweight control.  
  
Although the number of ‘false positive’ alerts generated by the EMVS has reduced significantly over time, the burden of alert management is still significant.  We believe that most alerts arise from poorly designed software and poorly configured barcode scanners.  
  
Solidsoft Reply cannot solve this issue, but we can contribute to the reduction of false-positive alerts by providing software that eliminates common problems.  This is what the calibrator does, in conjunction with our EMVS barcode parser.  The parser, in turn, depends on an underlying High Capacity AIDC (Automatic Identification and Data Capture) parser which has more general application, and which further relies on GS1 and ASC MH 10.8.2 data parsers.  The calibration library is not tied to the EMVS and can be used widely to support many different scenarios across different industries and supply chains.  
## How to use the Calibrator
The library generates a sequence of one or more barcodes which can be displayed to the end user and scanned.  Use the CalibrationTokens() method to obtain an enumerable collection of tokens.  Each token provides the next barcode as a bitmap stream.  Use the BitmapStream property of the token. 
  
Each time, the end-user scans a barcode, pass the data reported to the client application to the Calibrator for analysis.  You do this by calling the Calibrate() method.  The very first barcode is called the ‘baseline’ barcode.  Following analysis of the reported baseline data, the Calibrator may yield additional barcodes.  This implies that the number of Calibration Tokens in the collection may change after the first call to Calibrate().  
  
Sometimes, you may want to create barcode images externally to the Calibration library. For example, you might want to create the images locally in a browser using a JavaScript library.  In this case, use the BaselineBarcodeData() method to get the textual content of the initial baseline barcode and pass this to some external barcode creation library to create a barcode image.  When the user scans the barcode, pass the reported data to Calibrate().  Then use the SupplementalBarcodeData() method to get a collection of any additional barcodes.  Iterate through the collection, creating barcodes and calling Calibrate for the reported data for each barcode.   You can use the Remaining property of the token to discover how many barcodes must still be scanned during a calibration session.  
  
Once all the barcodes have been scanned, and the resulting data has been analysed, the Calibrator will provide the results of the analysis to the client application.  This includes detailed information about any issues which were detected together with a summary of the system capabilities.  By ‘system’, we mean the combination of the barcode scanner, operating system and client software.  If the Calibrator can successfully translate reported characters into data that correctly represents the contents of a scanned barcode, it provides additional calibration data which should generally be saved (as JSON) and used in each subsequent user session.  
  
The CalibrationData property provides all the information required to map individual characters and character sequences reported by the system to the data actually recorded in the barcode.  To perform this transformation, all that is necessary is to instantiate the Calibrator over a set of calibration data and then call the ProcessInput() method, passing the barcode data reported by the system.  The method returns the transformed data.  
## The stateless calibrator
The library provides a ‘stateless’ calibrator.  This is similar to the Calibrator class.  However, the stateless calibrator provides a richer set of state inside each token, recording all state that must be persisted across a calibration session.  Use the stateless calibrator when there is no guarantee that the calibrator instance will be retained across a session involving multiple barcodes.  Fort example, if a client application calls a web service to obtain the next token and to report data for calibration, you will generally not wish to affinitise each call in the session to the same Calibration instance.  Use the StatelessCalibration instead.  The StatelessCalibration class does not implement a CalibrationTokens() method.  Instead, use the NextCalibrationToken() method to get the next token to be scanned, returning the current token each time.  The first call will not include any existing token and will return a token for the baseline barcode.  You can serialise the content of each calibration token to JSON for exchange between a server and a client and pass the token either in the body of a request or response or in a header.  
  
## Representing reported data
The Calibrator adapts your application software to the ‘system’ comprising of the barcode scanner, acting as a keyboard device, and the computer’s operating system which handles keyboard input.  This allows you to transform the reported data (the data passed from the OS to the client application) to the original data held in the barcode.  However, you must take care when representing the reported data to the Calibration library.  
  
The basic principle is that ‘every keystroke counts’.  This is important in order to maximise the opportunity for the Calibrator to determine a reliable transformation approach that will correctly yield the original data for all barcodes.  It means that all keystrokes should be represented in the data passed to the Calibrator, even where those keystrokes do not map to any character.  
  
There are a number of reasons why a keystroke may not map to a character.  The main reasons are as follows:  
  
&ensp;a)	The scanner emits a keystroke (possibly in combination with the Shift or AltGr keys) which does not represent any character in the keyboard layout supported by the OS.  
  
&ensp;b)	The scanner emits a keystroke combination (often Ctrl + <key>) to represent a non-printable ASCII control character (e.g., ASCII 29).  However, this combination is not recognised in the keyboard layout supported by the OS.  This is really a specialised example of a).  
  
&ensp;c)	The scanner emits a keystroke that is interpreted as a ‘dead key’ keystroke in the keyboard layout supported by the OS.  A ‘dead key’ is a key that, when pressed, does not emit a character.  Instead, it modifies the character produced by the subsequent keystroke.  For example, pressing the apostrophe key on some keyboard layouts may modify the next keystroke by emitting an accented form of the character represented by that second keystroke.  
  
In all these cases, you should represent the character-less keystroke by appending an ASCII 0 (NULL) to the data input that you give to the Calibrator.  This approach should be taken when calling Calibrate() during a calibration session and when calling ProcessInput() when scanning a barcode in normal usage.  If you are unable for any reason to emit ASCII 0 characters in this way, you will significantly reduce the capability of the library to find a reliable transformation strategy, although this may still be possible, depending on the keyboard layouts configured in the scanner and the OS.  
  
As an example, consider a barcode that contains the following sequence of characters:  
  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;'a  
  
The barcode scanner has its default (factory) configuration as a US keyboard layout.  However, the OS is configured for a Spanish keyboard.  On a Spanish keyboard, the ' key is a dead key which modifies a subsequent vowel.  Therefore, the reported data is:  
  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;á  
  
Two keystrokes map to a single character.  In the data that you pass to the calibrator, you should pass an ASCII 0 to represent the dead key:  
  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;\u0000á  
  
You should be able to do this by detecting the dead key keyboard event(s).
We recommend that you always take this approach, even if the dead key does not modify the following keystroke.  Consider the following character sequence in the barcode:  
  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;'b  
  
In this case, when the OS interprets the keystrokes for a Spanish keyboard, the second keystroke) for the letter ‘b’) is not modified.  Instead, the OS outputs two characters for the second keystroke, as follows: 
  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;'b  
  
Although this directly represents the two characters in the barcode, we recommend that you report the following to the Calibrator:  
  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;\u0000'b  
  
The Calibrator has some inherent limitations:  
  
&ensp;a)	It is generally not suitable for use in situations where your client software depends on some underlying UI framework that hides keystrokes that do not map to a character.  Fortunately, this is rare.  Most UI frameworks provide keyboard events for each key, and report an ASCII 0 or a ‘dead key’ flag for dead keys.  
  
&ensp;b)	It is infeasible to test every possible character combination that could occur in a given keyboard layout.  The Calibrator only tests key combinations for ASCII characters.  If a barcode contains extended ASCII characters or other UNICODE characters, the data may not always be reliably transformed by the ProcessInput() method.  For this reason, you can optionally provide pre-processor component instances to the Calibrator, both during a calibration session (calls to the Calibrate() method) and on each call to ProcessInput().  Pre-processors are chained, and each pre-processor can re-write the input string, as required, to handle non-ASCII characters.  NB., several data formatting standards, including the GS1 standards, limit data to ASCII characters or a subset of ASCII (e.g., the ISO/IEC 646 Invariant character set comprising 82 ASCII characters).  Although the Calibrator is designed for general use over the entire ASCII character set, it is primarily orientated towards the GS1 standards which are constrained to use the Invariant ASCII subset. 
  
&ensp;c)	The calibrator cannot work in situations where the barcode scanner uses a keyboard layout that cannot represent characters in the barcode.  For example, if the barcode scanner is configured to support a Cyrillic keyboard layout, it will be unable to represent ASCII’s Latin characters.  Many barcode scanners support a different input approach which can be used in these scenarios.  This configuration causes the barcode scanner to emulate a numeric keypad to enter character codes.  This is less performant, but can still work with the Calibrator.  
  
The Calibrator can recognise and handle additional characters emitted by the barcode scanner which are not present in the scanned barcodes.  This is true both during calibration sessions (calls to Calibrate()) and calls to ProcessInput().  The Calibrator recognises the following:
* Prefixes prepended to the start of the reported data
  
* Suffixes appended to the end of the reported data
  
* AIM identifiers, indicating the barcode symbology.
  
* Additional codes or labels that may be appended after the AIM identifier and before the barcode data.
  
The Calibrator cannot distinguish between codes or labels appended to the barcode data before a suffix, and that suffix.  In this case, the code or label is simple treated as part of the suffix.
  
The Calibrator handles keyboard-defined ligatures.  For example, consider the scenario where the OS is configured to use an Arabic keyboard and the barcode scanner is configured for a US keyboard, and a barcode contains the following character:  
  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;b  
   
This character is mapped to the لا character by the OS.  However, لا is really a ligature composed from two characters, ل and ا.  Most UI frameworks report these two characters independently of the ligature character.  For example, in a browser application, you will receive two keypress events (one for each character) as well as two input events.  The second input will represent the ligature character.  However, you should simply pass the two underlying characters to the Calibrator during a Calibration session which will correctly recognise that the sequence represents a ligature.  
  
Although rare, it is sometimes that case that a character in a barcode corresponds to a dead key character in the barcode scanner’s keyboard layout.  By convention, the ‘literal’ dead key character is entered by emitting the dead key keystroke followed by a space.  The Calibrator handles these scenarios, even detecting situations that occur on certain keyboards where another character is substituted for the ‘literal’ character.  
  
In very rare situations, a sequence of two characters that appear in a barcode may be reversed in the order they are reported by the OS.  This only happens under very specific conditions which are detected by the Calibrator.  You do not need to detect these conditions yourself.  Simply input the characters to the Calibrator in the order they are reported by the UI framework.  
  
The Calibrator does not provide transformations for ASCII control characters except for the following which are commonly used as delimiters in barcodes:  
*	ASCII 28 (File Separator) – Used as a segment terminator in EDI data.
  
*	ASCII 29 (Group Separator) – Widely used to represent FNC1 codes embedded in barcodes.
  
*	ASCII 30 (Record Separator) – Used as a format section delimiter in barcodes that adhere to the syntax described in ISO/IEC 15434.
  
*	ASCII 31 (Unit Separator) – Used as a sub-element separator in EDI data
The Calibrator also handles ASCII 04 (End-of-Transmission) which is sometimes used to delimit the end of the barcode data.
  
For any other ASCII characters, either pass the reported character, as-is, to the Calibrator, which will not map it, or introduce a pre-processor component to re-write or discard these characters before they are analysed by the Calibrator.
  
Please note that the Calibrator is not designed to work with IMEs.  
## When Calibration fails
The most natural approach is to calibrate your client software based on the reported characters emitted by the OS when a barcode is scanned.  However, there are several reasons why the calibrator may not be able to find a reliable transformation strategy, including the following:  
  
&ensp;a)	Two different characters in the barcode may map to the same character at the OS level.  
  
&ensp;b)	Two different ‘dead key’ sequences may map to the same character at the OS level.  
  
&ensp;c)	The barcode scanner may not support data entry for one or more characters in a barcode.  
  
&ensp;d)	The barcode scanner may not support entry of ASCII control characters.  
  
&ensp;e)	The OS keyboard layout may not support the input of ASCII control characters.  
  
There are several variations of these issues.  During a calibration session, the Calibrator analyses data input carefully to detect many different problems that could arise.  If it finds issues that prevent it from creating a reliable transformation strategy, it reports these as errors.  The Calibrator is ‘opinionated’.  It favours the use of barcodes that adhere to GS1 and ANSI syntax standards.  By default, it ignores certain scenarios that should never apply to GS1 FNC1 barcodes.  GS1 FNC1 barcodes contain data that is formatted according to GS1 standards using Application Identifiers (AIs).  They use a special code (232) to represent delimiters. The barcode scanner maps this to an ASCII 29 (since 2018, GS1 FNC1 barcodes may contain literal ASCII 29 characters instead of the special code).  
  
Use the IncludeFormatnnTests property to include additional analysis during a calibration session.  If this property is set to ‘true’, the Calibrator will test for the use of other control characters (ASCII 28, 30 and 31).  These control characters are used in barcodes that conform to various formats specified in the ISO/IEC 15434:2019 standards.  These include Format 05 barcodes that start with GS1 AI data, but may include additional data in other formats, and Format 06 barcodes that start with ANSI MH10.8.2 Data Identifier (DI) data and which may include additional data in other formats.  
  
Regardless of the IncludeFormatnnTests setting, the Calibrator always tests the entire range of ASCII printable characters, and therefore can be used to calibrate software to handle a much wider range of barcode types and data syntax.  
  
If the Calibrator cannot find a reliable transformation strategy based on reported ASCII characters, you have a couple of options:  
*	Configure the barcode scanner to emulate a numeric keypad, rather than a keyboard.  Many scanners provide this as a configurable option.  In this mode, the scanner will enter characters as numeric codes in which each digit is represented by a different keystroke.  This may require end users to configure their barcode scanner, and is a significantly slower approach to data entry.  In any case, the Calibrator may not represent any additional value in this case.
  
*	Consider reporting scan codes rather than characters.  Scan codes represent the ‘physical’ keys on a keyboard, rather than the character to which that key is mapped by the OS. Scan codes may be virtualised by the OS to cater for different keyboard layouts and they are unlikely to be the same across different OS platforms.  Modern browsers provide an abstract cross-platform representation of scan codes via the KeyboardEvent ‘code’ property.  You may need to map these codes to character codes to represent them as unique characters to the Calibrator.  This approach is powerful.  It can potentially maximise the ability of the Calibrator to find a transformation strategy in the widest range of scenarios.  However, scan codes may not always be available to your client software, depending on the underlying UI framework that it uses.  For example, scan codes are generally not available in terminal (console) applications.  
## Advice
The Calibrator provides a default implementation of an Advice class.  Advice is composed of an ordered sequence of advice items.  Each item describes a condition and provides zero or more advice entries suitable for display to an end user.  You can display this information to the end user at the end of a calibration session (i.e., once all the calibration barcodes in the session have been scanned).  
  
To create advice call the CreateAdvice() factory method of the Advice class, passing in the SystemCapabilities object provided by the last token returned in a calibration session.  The method returns a new Advice instance populated with information.  
  
You can create custom Advice classes by implementing the IAdvice interface.  
## Controlling barcode size
The Calibrator creates 2D Data Matrix barcode images during each calibration session.  A future version may introduce support for other symbologies (e.g., QR codes).  Barcode calibration is performed in a platform-independent manner.  However, there may be occasions when it is more convenient to create barcode images externally using some other library.  This was discussed earlier, and this approach may be used today to support other symbologies.  
  
The barcodes created by the Calibrator contain all the information required to analyse the behaviour of the system and to search for a reliable transformation strategy.  Each barcode may be fairly large due to the amount of information it holds.  This is not a problem for most modern scanners, although it is possible that certain scanners could struggle to read the barcodes.  Some ‘legacy’ browsers such as Microsoft Internet Explorer and the original version of Microsoft Edge (now replaced by a Chromium-based browser of the same name) cannot read larger barcodes easily.  These older browsers implement internal keyboard input caching which makes it increasingly difficult to handle barcode scanner input as the barcode size increases.  These older browsers often only capture part of the barcode input, discarding the remaining keyboard events.  These issues have not been observed with modern browsers, including the current version of Microsoft Edge.  
  
Although we recommend that developers no longer support older and unsupported browsers, it may still sometimes be necessary to minimise the size of the calibration barcodes to make it easier to scan them.  This can be done by setting the barcode size when calling the CalibrationTokens() method or, for stateless calibration, the NextCalibrationToken() method.  These methods support a range of standard Data Matrix barcode sizes.  
  
When a size is selected that is too small to hold all the calibration data, the Calibrator automatically segments the data into shorter sections and emits a larger number of Calibration tokens and corresponding barcode images.  The end user may need to scan many more barcodes during a calibration session.  The Calibrator cannot assess the total number of barcodes for certain until it has analysed the ‘baseline’ data which may now be held in a sequence of more than one barcodes.  Therefore, the remaining number of barcodes may initially be reported as a smaller number and then jump to a larger number, if required.  
  
You can still use an external barcode image generator.  When you call the BaselineBarcodeData() or SupplementalBarcodeData() methods, you can specify the required Data Matrix size in order to segment the data.  The segmented data is returned as a collection (a list for BaselineBarcodeData(), and a dictionary of lists for SupplementalBarcodeData() where each key is a character representing a dead key).  
  
NB. The current version of the Calibrator does not support the generation of inverse barcodes.  Support for this may be added to a later version.  Use an external barcode image generator to create inverse barcodes.  
