// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeyboardCalibratorTestsInformation.cs" company="Solidsoft Reply Ltd.">
//   (c) 2018 Solidsoft Reply Ltd.  All rights reserved.
// </copyright>
// <summary>
// Unit tests for the Keyboard Calibrator
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Solidsoft.Reply.BarcodeScanner.Calibration.Tests;

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Calibration;
using DataMatrix;

/// <summary>
/// Unit tests for the Keyboard Calibrator.
/// </summary>
public class KeyboardCalibratorTestsInformation {
    /// <summary>
    /// Reported data for testing that the reported character is ambiguous. There are multiple keys for the
    /// same character, each representing a different expected character.
    /// </summary>
    private const string MultipleKeys = "  ! \" % & ' ( ) * 0 , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";

    /// <summary>
    /// Reported data for testing that the reported character sequence {0} is ambiguous. The same sequence
    /// is reported for multiple expected character sequences.
    /// </summary>
    private const string MultipleSequences = "  ! \" % & ' ( ) * ោះ , - . / ោះ 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";

    /// <summary>
    /// Reported data for testing that the reported character sequence {0} is ambiguous. The same sequence
    /// is reported for multiple dead keys in the scanner's keyboard layout.
    /// </summary>
    private const string MultipleSequencesForScannerDeadKey = "  ! \" % & ' ( ) * \0  , \0  . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";

    /// <summary>
    /// Reported data for testing that the reported character {0} is ambiguous. Unique pack identifiers
    /// cannot be read reliably.
    /// </summary>
    private const string GroupSeparatorMappingA = "  1 % 5 7 ù 9 0 8 + ; ) : ! à & é \" ' ( - è _ ç M m . \0^/ § Q B C D E F G H I J K L ? N O P A R S T U V Z X Y W ° q b c d e f g h i j k l , n o p a r s t u v z x y w   3 4 2 ^ * $ 6 ² \0¨µ £ \0    \0    \0    \0    \0    \x000D";
    private const string GroupSeparatorMappingADeadKey1 = "\0^1\0^%\0^3\0^4\0^5\0&\0^ù\0^9\0^0\0^8\0^+\0^;\0^)\0^:\0^!\01\0^&\0^é\0^\"\0^'\0^(\0^-\0^è\0^_\0^ç\0^M\0^m\0^.\0^=\0^/\0^§\0^2\0^Q\0^B\0^C\0^D\0Ê\0^F\0^G\0^H\0Î\0^J\0^K\0^L\0^?\0^N\0Ô\0^P\0Â\0^R\0^S\0^T\0Û\0^V\0^Z\0^X\0^Y\0^W\0^^\0^*\0^$\0^6\0^°\0^²\0^q\0^b\0^c\0^d\0ê\0^f\0^g\0^h\0î\0^j\0^k\0^l\0^,\0^n\0ô\0^p\0â\0^r\0^s\0^t\0û\0^v\0^z\0^x\0^y\0^w\0^¨\0^µ\0^£\0\0^";
    private const string GroupSeparatorMappingB = "  + ä % / à ) = ( \0`, ' . - 0 1 2 3 4 5 6 7 8 9 ö é ; \0^: _ A B C D E F G H I J K L M N O P Q R S T U V W X Z Y ? a b c d e f g h i j k l m n o p q r s t u v w x z y   * ç \" è $ \0\"& § ü £ ! °    \0    \0    \0    \0    \x000D";
    private const string GroupSeparatorMappingBDeadKey1 = "\0`+\0`ä\0`*\0`ç\0`%\0`/\0`à\0`)\0`=\0`(\0``\0`,\01\0`.\0`-\0`0\0`1\0`2\0`3\0`4\0`5\0`6\0`7\0`8\0`9\0`ö\0`é\0`;\0`^\0`:\0`_\0`\"\0À\0`B\0`C\0`D\0È\0`F\0`G\0`H\0Ì\0`J\0`K\0`L\0`M\0`N\0Ò\0`P\0`Q\0`R\0`S\0`T\0Ù\0`V\0`W\0`X\0`Z\0`Y\0`è\0`$\0`¨\0`&\0`?\0`§\0à\0`b\0`c\0`d\0è\0`f\0`g\0`h\0ì\0`j\0`k\0`l\0`m\0`n\0ò\0`p\0`q\0`r\0`s\0`t\0ù\0`v\0`w\0`x\0`z\0`y\0`ü\0`£\0`!\0`°\x000D";
    private const string GroupSeparatorMappingC = "  ! @ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   £ $ \" [ # ] ^ ` { ~ } ¬    @    \x001C    \0    \0    \x000D";
    private const string GroupSeparatorMappingD = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \"    \x001C    \0    \0    \x000D";
    private const string ControlCharacterMappingA = "  1 % 5 7 ù 9 0 8 + ; ) : ! à & é \" ' ( - è _ ç M m . = / § Q B C D E F G H I J K L ? N O P A R S T U V Z X Y W ° q b c d e f g h i j k l , n o p a r s t u v z x y w   3 4 2 \0^* $ 6 ² \0¨µ £ \0    \0    \0    \0    \0    \x000D";
    private const string ControlCharacterMappingADeadKey1 = "\0^1\0^%\0^3\0^4\0^5\0^7\0^ù\0^9\0^0\0^8\0^+\0^;\0^)\0^:\0^!\0^à\0^&\0^é\03\0^'\0^(\0^-\0^è\0^_\0^ç\0^M\0^m\0^.\0^=\0^/\0^§\0^2\0^Q\0^B\0^C\0^D\0Ê\0^F\0^G\0^H\0Î\0^J\0^K\0^L\0^?\0^N\0Ô\0^P\0Â\0^R\0^S\0^T\0Û\0^V\0^Z\0^X\0^Y\0^W\0^^\0^*\0^$\0^6\0^°\0^²\0^q\0^b\0^c\0^d\0ê\0^f\0^g\0^h\0î\0^j\0^k\0^l\0^,\0^n\0ô\0^p\0â\0^r\0^s\0^t\0û\0^v\0^z\0^x\0^y\0^w\0^¨\0^µ\0^£\0\0^";        ////private const string UnitedKingdomExtendedDeadKey1 = "\0`!\0`@\0`£\0`$\0`%\0`&\0`'\0`(\0`)\0`*\0`+\0`,\0`-\0`.\0`/\0`0\0`1\0`2\0`3\0`4\0`5\0`6\0`7\0`8\0`9\0`:\0`;\0`<\0`=\0`>\0`?\0`\"\0À\0`B\0`C\0`D\0È\0`F\0`G\0`H\0Ì\0`J\0`K\0`L\0`M\0`N\0Ò\0`P\0`Q\0`R\0`S\0`T\0Ù\0`V\0Ẁ\0`X\0Ỳ\0`Z\0`[\0`#\0`]\0`^\0`_\0``\0à\0`b\0`c\0`d\0è\0`f\0`g\0`h\0ì\0`j\0`k\0`l\0`m\0`n\0ò\0`p\0`q\0`r\0`s\0`t\0ù\0`v\0ẁ\0`x\0ỳ\0`z\0`{\0`~\0`}\0`¬\x000D";
    private const string ControlCharacterMappingB = "  1 % 5 7 ù 9 0 8 + ; ) : ! à & é \" ' ( - è _ ç M m . = / § Q B C D E F G H I J K L ? N O P A R S T U V Z X Y W ° q b c d e f g h i j k l , n o p a r s t u v z x y w   3 4 2 \0^* $ 6 ² \0¨µ £ \0    \x001D    \0    \0    \0    \x000D";
    private const string ControlCharacterMappingBDeadKey1 = "\0^1\0^%\0^3\0^4\0^5\0^7\0^ù\0^9\0^0\0^8\0^+\0^;\0^)\0^:\0^!\0^à\0^&\0^é\0\"\0^'\0^(\0^-\0^è\0^_\0^ç\0^M\0^m\0^.\0^=\0^/\0^§\0^2\0^Q\0^B\0^C\0^D\0Ê\0^F\0^G\0^H\0Î\0^J\0^K\0^L\0^?\0^N\0Ô\0^P\0Â\0^R\0^S\0^T\0Û\0^V\0^Z\0^X\0^Y\0^W\0^^\0^*\0^$\0^6\0^°\0^²\0^q\0^b\0^c\0^d\0ê\0^f\0^g\0^h\0î\0^j\0^k\0^l\0^,\0^n\0ô\0^p\0â\0^r\0^s\0^t\0û\0^v\0^z\0^x\0^y\0^w\0^¨\0^µ\0^£\0\0^";
    private const string ControlCharacterMappingC = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    @    \x001C    \0    \0    \x000D";
    private const string ControlCharacterMappingD = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ ^ [ \\ ] @ ` { | } ~    @    ^    \0    \x001D    \x000D";
    private const string ControlCharacterMappingE = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ ^ [ \\ ] @ ` { | } ~    @    \x001C    ^    \0    \x000D";
    private const string LigatureControlMapping = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ ^ [ \\ ] @ ` { | } ~    µ²    \x001C    \x001E    \0    \x000D";
    private const string Format06GroupSeparatorMappingA = "  1 % 5 7 ù 9 0 8 + ; ) : ! à & é \" ' ( - è _ ç M m . = / § Q B C D E F G H I J K L ? N O P A R S T U V Z X Y W ° q b c d e f g h i j k l , n o p a r s t u v z x y w   3 4 2 \0^* $ 6 ² \0¨µ £ \0    \0    \0    \0    \0    \x000D";
    private const string Format06GroupSeparatorMappingADeadKey1 = "\0^1\0ç\0^3\0^4\0^5\0^7\0^ù\0^9\0^0\0^8\0^+\0^;\0^)\0^:\0^!\0^à\0^&\0^é\0^\"\0^'\0^(\0^-\0^è\0^_\09\0^M\0^m\0^.\0^=\0^/\0^§\0^2\0^Q\0^B\0^C\0^D\0Ê\0^F\0^G\0^H\0Î\0^J\0^K\0^L\0^?\0^N\0Ô\0^P\0Â\0^R\0^S\0^T\0Û\0^V\0^Z\0^X\0^Y\0^W\0^^\0^*\0^$\0^6\0^°\0^²\0^q\0^b\0^c\0^d\0ê\0^f\0^g\0^h\0î\0^j\0^k\0^l\0^,\0^n\0ô\0^p\0â\0^r\0^s\0^t\0û\0^v\0^z\0^x\0^y\0^w\0^¨\0^µ\0^£\0\0^";        ////private const string UnitedKingdomExtendedDeadKey1 = "\0`!\0`@\0`£\0`$\0`%\0`&\0`'\0`(\0`)\0`*\0`+\0`,\0`-\0`.\0`/\0`0\0`1\0`2\0`3\0`4\0`5\0`6\0`7\0`8\0`9\0`:\0`;\0`<\0`=\0`>\0`?\0`\"\0À\0`B\0`C\0`D\0È\0`F\0`G\0`H\0Ì\0`J\0`K\0`L\0`M\0`N\0Ò\0`P\0`Q\0`R\0`S\0`T\0Ù\0`V\0Ẁ\0`X\0Ỳ\0`Z\0`[\0`#\0`]\0`^\0`_\0``\0à\0`b\0`c\0`d\0è\0`f\0`g\0`h\0ì\0`j\0`k\0`l\0`m\0`n\0ò\0`p\0`q\0`r\0`s\0`t\0ù\0`v\0ẁ\0`x\0ỳ\0`z\0`{\0`~\0`}\0`¬\x000D";
    private const string Format06GroupSeparatorMappingB = "  + ä % / ù ) = ( \0`, ' . - à & é 3 4 5 6 è 8 9 ö µ ; \0^: _ A B C D E F G H I J K L M N O P Q R S T U V W X Z Y ? a b c d e f g h i j k l m n o p q r s t u v w x z y   * ç \" # $ \0\"² § ü £ ! °    \0    \0    \0    \0    \x000D";
    private const string Format06GroupSeparatorMappingBDeadKey1 = "\0`+\0`ä\0`*\0`ç\0`%\0`/\0`à\0`)\0`=\0`(\0``\0`,\0`'\0`.\0`-\0`0\0`1\0`2\0`3\0`4\0`5\0`6\0`7\0`8\09\0`ö\0`é\0`;\0`^\0`:\0`_\0`\"\0À\0`B\0`C\0`D\0È\0`F\0`G\0`H\0Ì\0`J\0`K\0`L\0`M\0`N\0Ò\0`P\0`Q\0`R\0`S\0`T\0Ù\0`V\0`W\0`X\0`Z\0`Y\0`è\0`$\0`¨\0`&\0`?\0`§\0`a\0`b\0`c\0`d\0`e\0`f\0`g\0`h\0ì\0`j\0`k\0`l\0`m\0`n\0`o\0`p\0`q\0`r\0`s\0`t\0`u\0`v\0`w\0`x\0`z\0`y\0`ü\0`£\0`!\0`°\x000D";
    private const string Format06GroupSeparatorMappingC = "  ! @ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   £ $ \" [ # ] ^ ` { ~ } ¬    \0   \0    \0    \0    \x000D";
    private const string Format0506NotRecognisedA = "  1 % 5 7 ù 9 0 8 + ; ) : ! à & é \" ' ( - è _ ç M m . = / § Q B C D E F G H I J K L ? N O P A R S T U V Z X Y W ° q b c d e f g h i j k l , n o p a r s t u v z x y w   3 4 2 \0^* $ 6 ² \0¨µ £ \0    \x001D    \0    \0    \0    \x000D";
    private const string Format0506NotRecognisedADeadKey1 = "\0^1\0^%\0^3\0^4\0^5\0^7\0^ù\0^9\0^0\0^8\0^+\0^;\0^)\0^:\0^!\0à\0^&\0^é\0^\"\0^'\0^(\0^-\0^è\0^_\0^ç\0^M\0^m\0^.\0^=\0^/\0^§\0^2\0^Q\0^B\0^C\0^D\0Ê\0^F\0^G\0^H\0Î\0^J\0^K\0^L\0^?\0^N\0Ô\0^P\0Â\0^R\0^S\0^T\0Û\0^V\0^Z\0^X\0^Y\0^W\0^^\0^*\0^$\0^6\0^°\0^²\0^q\0^b\0^c\0^d\0ê\0^f\0^g\0^h\0î\0^j\0^k\0^l\0^,\0^n\0ô\0^p\0â\0^r\0^s\0^t\0û\0^v\0^z\0^x\0^y\0^w\0^¨\0^µ\0^£\0\0^";
    private const string Format0506NotRecognisedB = "  ! @ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   £ $ \" [ # ] ^ \0`{ ~ } ¬    \x001D    \0    \0    \0    \x000D";
    private const string Format0506NotRecognisedADeadKey2 = "\0`!\0`@\0`£\0`$\0`%\0`&\0`'\0`(\0`)\00\0`+\0`,\0`-\0`.\0`/\0`0\0`1\0`2\0`3\0`4\0`5\0`6\0`7\0`8\0`9\0`:\0`;\0`<\0`=\0`>\0`?\0`\"\0À\0`B\0`C\0`D\0È\0`F\0`G\0`H\0Ì\0`J\0`K\0`L\0`M\0`N\0Ò\0`P\0`Q\0`R\0`S\0`T\0Ù\0`V\0Ẁ\0`X\0Ỳ\0`Z\0`[\0`#\0`]\0`^\0`_\0``\0à\0`b\0`c\0`d\0è\0`f\0`g\0`h\0ì\0`j\0`k\0`l\0`m\0`n\0ò\0`p\0`q\0`r\0`s\0`t\0ù\0`v\0ẁ\0`x\0ỳ\0`z\0`{\0`~\0`}\0`¬\x000D";
    private const string Format0506NotRecognisedC = "  1 % 5 7 ù 9 0 8 + ; ) : ! à & é \" ' ( - è _ ç M m . = / § Q B C D E F G H I J K L ? N O P A R S T U V Z X Y W ° q b c d e f g h i j k l , n o p a r s t u v z x y w   3 4 2 \0^* $ 6 ² \0¨µ £ \0    \x001D    \x001C    \0^    \0    \x000D";
    private const string Format0506NotRecognisedD = "  1 % 5 7 ù 9 0 8 + ; ) : ! à & é \" ' ( - è _ ç M m . = / § Q B C D E F G H I J K L ? N O P A R S T U V Z X Y W ° q b c d e f g h i j k l , n o p a r s t u v z x y w   3 4 2 [ * $ [ ² \0¨µ £ \0    \x001D    \x001C    \0    \0    \x000D";
    private const string Format0506NotReliablyRecognised1 = "  1 % 5 7 ù 9 0 8 + ; ) : ! à & é \" ' ( - è _ ç M m . = / § Q B C D E F G H I J K L ? N O P A R S T U V Z X Y W ° q b c d e f g h i j k l , n o p a r s t u v z x y w   3 4 2 \0^* $ 6 ² \0¨µ £ \0    \x001D    \x001C    \0    \0^    \x000D";
    private const string IncompatibleScannerDeadKey = "  ! \" % & ' ( ) * \0  , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x \0  z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string NoDelimiters = "! @ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   £ $ \" [ # ] ^ ` { ~ } ¬    \x001D    \x001C    \0    \0    \x000D";
    private static readonly string NoTemporaryDelimiterCandidateA = GetAllExtendedAscii() + "   £ $ \" [ # ] ^ ` { ~ } ¬    \x001D    \x001C    \0    \0    \x000D";
    private const string NoTemporaryDelimiterCandidateB = "  1 % 5 7 ù 9 0 8 + ; ) : ! à & é \" ' ( - è _ ç M m . = / § Q B C D E F G H I J K L ? N O P A R S T U V Z X Y W ° q b c d e f g h i j k l , n o p a r s t u v z x y w   3 4 2 \0^* $ 6 ² \0¨µ £ \0    \x001D    \x001C    \0    \0    \x000D";
    private static readonly string NoTemporaryDelimiterCandidateBDeadKey1 = GetAllExtendedAsciiDeadKey('^');
    private const string DeadKeyMultipleKeys = "  ª Ç ) ! ç ? ₧ ¿ \0¨, - . = 0 1 2 3 4 5 6 7 8 9 Ñ ñ ; \0¨: % A B C D E F G H I J K L M N O P Q R S T U V W X Y Z + a b c d e f g h i j k l m n o p q r s t u v w x y z   / ( \" ÷ \0´\0`¡ ' × \0´\0`·    \x001D    \x001C    \0    \0    \x000D";
    private const string DeadKeyMultiMapping = "  ! \0\"% & \0'( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string DeadKeyMultiMappingDeadKey1 = "\0\"!\0\"\"\0\"#\0§\0°\0±\0\"'\0\"(\0\")\0×\0\"+\0\",\0—\0\".\0\"/\0\"0\0\"1\0\"2\0\"3\0§\0°\0\"6\0±\0×\0\"9\0\":\0\";\0\"<\0\"=\0\">\0\"?\0\"@\0Ā\0\"B\0Č\0\"D\0Ē\0\"F\0Ģ\0\"H\0Ī\0\"J\0Ķ\0Ļ\0\"M\0Ņ\0Ō\0\"P\0\"Q\0Ŗ\0Š\0\"T\0Ū\0\"V\0\"W\0\"X\0\"Y\0Ž\0\"[\0\"\\\0\"]\0\"^\0—\0\"`\0Ā\0\"b\0Č\0\"d\0Ē\0\"f\0Ģ\0\"h\0Ī\0\"j\0Ķ\0Ļ\0\"m\0Ņ\0Ō\0\"p\0\"q\0Ŗ\0Š\0\"t\0Ū\0\"v\0\"w\0\"x\0\"y\0Ž\0\"{\0\"|\0\"}\0\"~\x000D";
    private const string NoGroupSeparatorMapping = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~        \x001C    \0    \0    \x000D";
    private const string MultipleKeysMultipleAsciiCharacters = "  ! @ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   ~ $ \" [ # ] ^ ` { ~ } ¬    \x001D    \x001C    \0    \0    \x000D";
    private const string SomeDeadKeyCombinationsUnrecognised = "  ! \0¨% & ' ( ) * \0+, - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   £ $ \" [ # ] ^ ` { ~ } ¬    \0    \0    \0    \0    \x000D";
    private const string SomeDeadKeyCombinationsUnrecognisedDeadKey1 = "\0¨!\0¨¨\0¨·\0¨$\0¨%\0¨/\0¨´\0¨)\0¨=\0¨(\0¨¿\0¨\0¨'\0¨.\0¨-\0¨0\0¨1\0¨2\0¨3\0¨4\0¨5\0¨6\0¨7\0¨8\0¨9\0¨Ñ\0¨ñ\0¨;\0¨¡\0¨:\0¨_\0¨\"\0Ä\0¨B\0¨C\0¨D\0Ë\0¨F\0¨G\0¨H\0Ï\0¨J\0¨K\0¨L\0¨M\0¨N\0Ö\0¨P\0¨Q\0¨R\0¨S\0¨T\0Ü\0¨V\0¨W\0¨X\0¨Y\0¨Z\0¨`\0¨ç\0¨+\0¨&\0¨?\0¨º\0ä\0¨b\0¨c\0¨d\0ë\0¨f\0¨g\0¨h\0ï\0¨j\0¨k\0¨l\0¨m\0¨n\0ö\0¨p\0¨q\0¨r\0¨s\0¨t\0ü\0¨v\0¨w\0¨x\0ÿ\0¨z\0¨^\0¨Ç\0¨*\0¨ª\x000D";
    private const string SomeDeadKeyCombinationsUnrecognisedDeadKey2 = "\0+!\0+\"\0+#\0+$\0+%\0+&\0+'\0+(\0+)\0+*\0++\0+,\0+\0῟\0+/\0+0\0+1\0+2\0+3\0+4\0+5\0+6\0+7\0+8\0+9\0+¨\0+΄\0+<\0+=\0+>\0+?\0+@\0Ἇ\0+Β\0+Ψ\0+Δ\0+Ε\0+Φ\0+Γ\0Ἧ\0Ἷ\0+Ξ\0+Κ\0+Λ\0+Μ\0+Ν\0+Ο\0+Π\0+:\0+Ρ\0+Σ\0+Τ\0+Θ\0Ὧ\0+΅\0+Χ\0Ὗ\0+Ζ\0+[\0+\\\0+]\0+^\0+_\0+~\0ἇ\0+β\0+ψ\0+δ\0+ε\0+φ\0+γ\0ἧ\0ἷ\0+ξ\0+κ\0+λ\0+μ\0+ν\0+ο\0+π\0+;\0+ρ\0+σ\0+τ\0+θ\0ὧ\0+ς\0+χ\0ὗ\0+ζ\0+{\0+|\0+}\0+`\x000D";
    private const string SomeCharactersUnrecognised = "  ! @ % & ' ( ) \0 + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   £ $ \" [ # ] ^ ` { ~ } ¬    \x001D    \x001C    \0    \0    \x000D";
    private const string UndetectedUniqueIdentifierCharacters = "  ! @ % & ' ( ) + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   £ $ \" [ # ] ^ ` { ~ } ¬    \x001D    \x001C    \0    \0    \x000D";
    private const string TooManyCharactersDetectedA = "  ! @ % & ' ( ) á * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   £ $ \" [ # ] ^ ` { ~ } ¬    \x001D    \x001C    \0    \0    \x000D";
    private const string TooManyCharactersDetectedB = "  ! @ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   £ á $ \" [ # ] ^ ` { ~ } ¬    \x001D    \x001C    \0    \0    \x000D";
    private const string TooManyCharactersDetectedC = "  ! \" % & ' ( ) * \0  , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z \0     # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string UnrecognisedData = "abcdefghijklmnopqrstuvwxyz";
    private const string NoCalibrationDataProvided = "  ! @ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   £ $ \" [ # ] ^ ` { ~ } ¬    \x001D    \x001C    \0    \0    \x000D";
    private const string NoCalibrationDataReportedA = "";
    private const string NoCalibrationDataReportedB = "  ! @ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   £ $ \" [ # ] ^ \0`{ ~ } ¬    \0    \0    \0    \0    \x000D";
    private const string NoCalibrationDataReportedCDeadKey1 = "";
    private const string NoCalibrationDataReportedD1 = "]d1  ! @ % & ' ( ) * + , - . / 0 1 2 3 4 5 6\r";
    private const string NoCalibrationDataReportedD2 = "]d1 7 8 9 : ; < = > ? A B C D E F G H I J K L\r";
    private const string NoCalibrationDataReportedD3 = "]d1 M N O P Q R S T U V W X Y Z _ a b c d e f\r";
    private const string NoCalibrationDataReportedD4 = "]d1 g h i j k l m n o p q r s t u v w x y z\r";
    private const string NoCalibrationDataReportedD5 = "]d1   £ $ \" [ # ] ^ \0`{ ~ }\r";
    private const string NoCalibrationDataReportedD6 = "]d1 ¬    \0    \0    \0       \r";
    private const string NoCalibrationDataReportedDeadKey11 = "]d1";
    private const string NoCalibrationDataReportedDeadKey12 = "]d1";
    private const string NoCalibrationDataReportedDeadKey13 = "]d1";
    private const string NoCalibrationDataReportedDeadKey14 = "]d1";
    private const string NoCalibrationDataReportedDeadKey15 = "]d1";
    private const string UnrecognisedDataD1 = "]d2  ! @ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 \x000D";
    private const string UnrecognisedDataD2 = "]d27 8 9 : ; < = > ? A B C D E F G H I J K L \x000D";
    private const string UnrecognisedDataD3 = "]d2M N O P Q R S T U V W X Y Z _ a b c d e f \x000D";
    private const string UnrecognisedDataD4 = "]d2g h i j k l m n o p q r s t u v w x y z   \x000D";
    private const string UnrecognisedDataD5 = "]d2£ $ \" [ # ] ^ \0`{ ~ } ¬    \x000D";
    private const string UnrecognisedDataD6 = "]d2\0    \0    \0    \0";
    private const string UnrecognisedDataD7 = "]d2    \x000D";
    private const string UnrecognisedDataDeadKey11 = "]d2abcdefghijklmnopqrstuvwxyz";
    private const string UnrecognisedDataDeadKey12 = "]d2";
    private const string UnrecognisedDataDeadKey13 = "]d2";
    private const string UnrecognisedDataDeadKey14 = "]d2";
    private const string UnrecognisedDataDeadKey15 = "]d2";
    private const string UnrecognisedDataDeadKey16 = "]d2";
    private const string CapsLockProbablyOff = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \0    \x001E    \0    \x000D";
    private const string CapsLockProbablyOn = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? a b c d e f g h i j k l m n o p q r s t u v w x y z _ A B C D E F G H I J K L M N O P Q R S T U V W X Y Z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \0    \x001E    \0    \x000D";
    private const string NonCorrespondingKeyboardLayoutsRecordSeparator = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? Z Y X W V U T S R Q P O N M L K J I H G F E D C B A _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \0    \0    Ẁ    \0    \x000D";
    private const string NonCorrespondingKeyboardLayoutsGroupSeparator = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? Z Y X W V U T S R Q P O N M L K J I H G F E D C B A _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    Ö    \0    \0    \0    \x000D";
    private const string NonCorrespondingKeyboardLayouts = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? Z Y X W V U T S R Q P O N M L K J I H G F E D C B A _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string NonCorrespondingKeyboardLayoutsForCharacterSet82 = "  ! \" % & ' ( ) * + , - . / * 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \u001e    \0    \x000D";
    private const string NonCorrespondingKeyboardLayoutsAdditionalAsciiCharacters = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z    ~ { | } ` ^ ] \\ [ @ $ #    \x001D    \x001C    \0    \0    \x000D";
    private const string IncompatibleScannerDeadKeyAdditionalAsciiCharacters = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # \0  \0  [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string AmbiguousCharacterSequence = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ $ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string DeadKeyMultiMappingAdditionalAsciiCharactersA = "  ! \0\"% & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] \0^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string DeadKeyMultiMappingAdditionalAsciiCharactersADeadKey1 = "\0\"!\0\"\"\0\"#\0§\0°\0±\0\"'\0\"(\0\")\0×\0\"+\0\",\0—\0\".\0\"/\0\"0\0\"1\0\"2\0\"3\0§\0°\0\"6\0±\0×\0\"9\0\":\0\";\0\"<\0\"=\0\">\0\"?\0\"@\0Ā\0\"B\0Č\0\"D\0Ē\0\"F\0Ģ\0\"H\0Ī\0\"J\0Ķ\0Ļ\0\"M\0Ņ\0Ō\0\"P\0\"Q\0Ŗ\0Š\0\"T\0Ū\0\"V\0\"W\0\"X\0\"Y\0Ž\0\"[\0\"\\\0\"]\0\"^\0—\0\"`\0Ā\0\"b\0Č\0\"d\0Ē\0\"f\0Ģ\0\"h\0Ī\0\"j\0Ķ\0Ļ\0\"m\0Ņ\0Ō\0\"p\0\"q\0Ŗ\0Š\0\"t\0Ū\0\"v\0\"w\0\"x\0\"y\0Ž\0\"{\0\"|\0\"}\0\"~\x000D";
    private const string DeadKeyMultiMappingAdditionalAsciiCharactersB = "  ! @ % & ' ( ) * \0+, - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   £ $ \" [ # ] \0^` { ~ } ¬    \x001D    \x001C    \0    \0    \x000D";
    private const string DeadKeyMultiMappingAdditionalAsciiCharactersBDeadKey1 = "\0+!\0+@\0+£\0+$\0+%\0+&\0+'\0+(\0+)\0+*\0++\0+,\0+-\0+.\0+/\0+0\0+1\0+2\0+3\0+4\0+5\0+6\0+7\0+8\0+9\0+:\0+;\0+<\0+=\0+>\0+?\0+\"\0À\0+B\0+C\0+D\0È\0+F\0+G\0+H\0Ì\0+J\0+K\0+L\0+M\0+N\0Ò\0+P\0+Q\0+R\0+S\0+T\0Ù\0+V\0Ẁ\0+X\0Ỳ\0+Z\0+[\0+#\0+]\0+^\0+_\0+`\0à\0+b\0+c\0+d\0è\0+f\0+g\0+h\0ì\0+j\0+k\0+l\0+m\0+n\0ò\0+p\0+q\0+r\0+s\0+t\0ù\0+v\0ẁ\0+x\0ỳ\0+z\0+{\0+~\0+}\0+¬\x000D";
    private const string DeadKeyMultiMappingAdditionalAsciiCharactersBDeadKey2 = "\0^!\0^\"\0^#\0+\0°\0±\0^'\0^(\0^)\0×\0^+\0^,\0—\0^.\0^/\0^0\0^1\0^2\0^3\0§\0°\0^6\0±\0×\0^9\0^:\0^;\0^<\0^=\0^>\0^?\0^@\0Ā\0^B\0Č\0^D\0Ē\0^F\0Ģ\0^H\0Ī\0^J\0Ķ\0Ļ\0^M\0Ņ\0Ō\0^P\0^Q\0Ŗ\0Š\0^T\0Ū\0^V\0^W\0^X\0^Y\0Ž\0^[\0^\\\0^]\0^^\0—\0^`\0Ā\0^b\0Č\0^d\0Ē\0^f\0Ģ\0^h\0Ī\0^j\0Ķ\0Ļ\0^m\0Ņ\0Ō\0^p\0^q\0Ŗ\0Š\0^t\0Ū\0^v\0^w\0^x\0^y\0Ž\0^{\0^|\0^}\0^~\x000D";
    private const string MultipleKeysAimFlagCharacter = "¬d2  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? ¬ B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ¬ ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string MultipleKeysAdditionalAsciiCharactersA = "  ! @ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   9 $ \" [ # ] ^ ` { ~ } ¬    \x001D    \x001C    \0    \0    \x000D";
    private const string MultipleKeysAdditionalAsciiCharactersB = "  ! @ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   £ ° \" [ # ] ^ ` { ° } ¬    \x001D    \x001C    \0    \0    \x000D";
    private const string EndOfLineNotTransmitted = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    ";
    private const string SuffixTransmitted = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    suffix\x000D";
    private const string PrefixTransmittedA = "prefix  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string PrefixTransmittedB = "]d2prefix  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string PrefixTransmittedC = "prefix]d2  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string PrefixTransmittedD = "prefixad2  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ a ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string PrefixTransmittedE = "prefix]d2code  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string PrefixTransmittedF = "prefixad2cade  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ a ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string AimNotRecognised = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ \0 ^ ` { | } ~    \x001D    \0    \0    \0    \x000D";
    private const string AimNotTransmittedA = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string AimNotTransmittedB = "[d2  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string SomeAdditionalCharacterCombinationsUnrecognised = "  ! \0¨% & ' ( ) * \0+, - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   £ $ \" [ # ] ^ ` { ~ } ¬    \0    \0    \0    \0    \x000D";
    private const string SomeAdditionalCharacterCombinationsUnrecognisedDeadKey1 = "\0¨!\0¨¨\0¨·\0¨$\0¨%\0¨/\0¨´\0¨)\0¨=\0¨(\0¨¿\0¨\0¨'\0¨.\0¨-\0¨0\0¨1\0¨2\0¨3\0¨4\0¨5\0¨6\0¨7\0¨8\0¨9\0¨Ñ\0¨ñ\0¨;\0¨¡\0¨:\0¨_\0¨\"\0Ä\0¨B\0¨C\0¨D\0Ë\0¨F\0¨G\0¨H\0Ï\0¨J\0¨K\0¨L\0¨M\0¨N\0Ö\0¨P\0¨Q\0¨R\0¨S\0¨T\0Ü\0¨V\0¨W\0¨X\0¨Y\0¨Z\0¨`\0¨ç\0¨+\0¨&\0¨?\0¨º\0ä\0¨b\0¨c\0¨d\0ë\0¨f\0¨g\0¨h\0ï\0¨j\0¨k\0¨l\0¨m\0¨n\0ö\0¨p\0¨q\0¨r\0¨s\0¨t\0ü\0¨v\0¨w\0¨x\0ÿ\0¨z\0¨^\0¨Ç\0¨*\0¨ª\x000D";
    private const string SomeAdditionalCharacterCombinationsUnrecognisedDeadKey2 = "\0+!\0+\"\0+#\0+$\0+%\0+&\0+'\0+(\0+)\0+*\0++\0+,\0+-\0῟\0+/\0+0\0+1\0+2\0+3\0+4\0+5\0+6\0+7\0+8\0+9\0+¨\0+΄\0+<\0+=\0+>\0+?\0+@\0Ἇ\0+Β\0+Ψ\0+Δ\0+Ε\0+Φ\0+Γ\0Ἧ\0Ἷ\0+Ξ\0+Κ\0+Λ\0+Μ\0+Ν\0+Ο\0+Π\0+:\0+Ρ\0+Σ\0+Τ\0+Θ\0Ὧ\0+΅\0+Χ\0Ὗ\0+Ζ\0+[\0+\\\0+]\0+^\0+_\0+~\0ἇ\0+β\0+ψ\0+δ\0+ε\0+φ\0+γ\0ἧ\0ἷ\0+ξ\0+κ\0+λ\0+μ\0+ν\0+ο\0+π\0+;\0+ρ\0+σ\0+τ\0+θ\0ὧ\0+ς\0+χ\0ὗ\0+ζ\0+{\0+\0+}\0+`\x000D";
    private const string SomeAdditionalCharactersUnrecognisedA = "  ! @ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   \0 $ \" [ # ] ^ ` { ~ } ¬    \x001D    \x001C    \0    \0    \x000D";
    private const string SomeAdditionalCharactersUnrecognisedB = "  ! @ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   \0  $ @ [ \\ ] \0  ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string SomeCharactersUnreported = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | }    \x001D    \x001C    \0    \0    \x000D";
    private const string NonCorrespondingKeyboardLayoutsForAimIdentifier1 = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ¦ ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string NonCorrespondingKeyboardLayoutsForAimIdentifier2 = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ \0¦^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string KeyboardScriptCyrillic = "  ! Ч % : ч – № / € р - л б 0 1 2 3 4 5 6 7 8 9 М м Р . Л Б ѝ Ф Ъ А Е О Ж Г С Т Н В П Х Д З ы И Я Ш К Э У Й Щ Ю $ ь ф ъ а е о ж г с т н в п х д з , и я ш к э у й щ ю   + \" ? ц „ ; = ( Ц “ § )    \0    \0    \0    \0    \x000D";
    private const string KeyboardScriptGreek = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 ¨ ΄ < = > ? Α Β Ψ Δ Ε Φ Γ Η Ι Ξ Κ Λ Μ Ν Ο Π : Ρ Σ Τ Θ Ω ΅ Χ Υ Ζ _ α β ψ δ ε φ γ η ι ξ κ λ μ ν ο π ; ρ σ τ θ ω ς χ υ ζ   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string KeyboardScriptLatin = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \0    \0    \x001E    \0    \x000D";
    private const string RecordSeparatorSupported = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \0    \0    \x001E    \0    \x000D";
    private const string GroupSeparatorSupported = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \0    \0    \0    \x000D";
    private const string EndOfLineTransmitted = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string AimTransmitted = "]d2  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string AimSupported = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string ScannerMayCompensateForCapsLock = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string CapsLockOn = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? a b c d e f g h i j k l m n o p q r s t u v w x y z _ A B C D E F G H I J K L M N O P Q R S T U V W X Y Z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string ScannerMayConvertToUpperCase = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ A B C D E F G H I J K L M N O P Q R S T U V W X Y Z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string ScannerMayConvertToLowerCase = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? a b c d e f g h i j k l m n o p q r s t u v w x y z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string ScannerMayInvertCase = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? a b c d e f g h i j k l m n o p q r s t u v w x y z _ A B C D E F G H I J K L M N O P Q R S T U V W X Y Z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string SubOptimalScannerKeyboardPerformance = "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";
    private const string ChainedSequenceData = "  ! \" % & ' ( ) * \0\0+ , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    \x001D    \x001C    \0    \0    \x000D";

    /// <summary>
    /// Test a simple string
    /// </summary>
    [Fact]
    public void UnitedStatesToUnitedStates() {
        var calibrator = new Calibrator();
        var loopCount = 0;

        foreach (var token in calibrator.CalibrationTokens()) {
            calibrator.Calibrate(ConvertToCharacterValues(BaselineCalibrationUsUs()), token);
            loopCount++;
        }

        Assert.Equal(1, loopCount);
    }

    /// <summary>
    /// Test that the reported character sequence {0} is ambiguous. The same
    /// sequence is reported for multiple expected character sequences.
    /// </summary>
    [Fact]
    public void ErrorMultipleSequences() {
        // Calibration fails
        var token = PerformCalibrationTest("MultipleSequences");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.MultipleSequences);
    }

    /// <summary>
    /// Test that the reported character {0} is ambiguous. There are multiple keys for the same character, each 
    /// representing a different expected character.
    /// </summary>
    [Fact]
    public void ErrorMultipleKeys() {
        // Calibration fails
        var token = PerformCalibrationTest("MultipleKeys");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.MultipleKeys);
    }

    /// <summary>
    /// Test that the reported character sequence {0} is ambiguous. The same
    /// sequence is reported for multiple dead keys in the scanner's keyboard
    /// layout.
    /// </summary>
    [Fact]
    public void ErrorMultipleSequencesForScannerDeadKey() {
        // Calibration fails
        var token = PerformCalibrationTest("MultipleSequencesForScannerDeadKey");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.MultipleSequencesForScannerDeadKey);
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.IncompatibleScannerDeadKey);
    }

    /// <summary>
    /// Test that the reported character {0} is ambiguous. Unique pack identifiers
    /// cannot be read reliably.
    /// </summary>
    [Fact]
    public void ErrorGroupSeparatorMapping() {
        // Calibration fails
        var token = PerformCalibrationTest("GroupSeparatorMappingA");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.GroupSeparatorMapping);

        token = PerformCalibrationTest("GroupSeparatorMappingB");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.GroupSeparatorMapping);

        token = PerformCalibrationTest("GroupSeparatorMappingC");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.GroupSeparatorMapping);

        token = PerformCalibrationTest("GroupSeparatorMappingD");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.GroupSeparatorMapping);
    }

    /// <summary>
    /// Test that bar codes that use Format 05 or Format 06 syntax cannot be recognised.
    /// This includes German PPN pack identifiers.
    /// </summary>
    [Fact]
    public void WarningFormat0506NotRecognised() {
        var token = PerformCalibrationTest("Format0506NotRecognisedA");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.IsoIec15434SyntaxNotRecognised);

        token = PerformCalibrationTest("Format0506NotRecognisedB");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.IsoIec15434SyntaxNotRecognised);

        token = PerformCalibrationTest("Format0506NotRecognisedC");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.IsoIec15434SyntaxNotRecognised);

        token = PerformCalibrationTest("Format0506NotRecognisedD");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.IsoIec15434SyntaxNotRecognised);
    }

    /// <summary>
    /// Test that bar codes that use Format 05 or Format 06 syntax cannot all be reliably recognised.
    /// This includes German PPN pack identifiers.
    /// </summary>
    [Fact]
    public void WarningFormat0506NotReliablyRecognised() {
        var token = PerformCalibrationTest("Format0506NotReliablyRecognised1");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.IsoIec15434EdiNotReliablyReadable);
    }

    /// <summary>
    /// Test that the reported character {0} is ambiguous. This may prevent reading
    /// of any additional data included in a barcode.
    /// </summary>
    [Fact]
    public void WarningControlCharacterMapping() {
        var token = PerformCalibrationTest("ControlCharacterMappingA");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.ControlCharacterMappingAdditionalDataElements);

        token = PerformCalibrationTest("ControlCharacterMappingB");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.ControlCharacterMappingAdditionalDataElements);

        token = PerformCalibrationTest("ControlCharacterMappingC");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.ControlCharacterMappingAdditionalDataElements);

        token = PerformCalibrationTest("ControlCharacterMappingD");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.ControlCharacterMappingAdditionalDataElements);

        token = PerformCalibrationTest("ControlCharacterMappingE");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.ControlCharacterMappingAdditionalDataElements);
    }

    /// <summary>
    /// Test that a ligature representing a control character is correctly configured.
    /// </summary>
    [Fact]
    public void LigatureControl() {
        var token = PerformCalibrationTest("Ligature Control");
        Assert.Contains(token.CalibrationData?.LigatureMap ?? new Dictionary<string, char>(), kvp => kvp is { Key: "µ²", Value: '\u001d' });
    }

    /// <summary>
    /// Test that the character {0} cannot be represented reliably because of
    /// incompatibility with the keyboard layout.
    /// </summary>
    [Fact]
    public void ErrorIncompatibleScannerDeadKey() {
        // Calibration fails
        var token = PerformCalibrationTest("IncompatibleScannerDeadKey");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.IncompatibleScannerDeadKey);
    }

    /// <summary>
    /// Test that The reported character {0} is ambiguous. German PPN unique pack
    /// identifiers cannot be read reliably.
    /// </summary>
    [Fact]
    public void WarningFormat06GroupSeparatorMapping() {
        var token = PerformCalibrationTest("Format06GroupSeparatorMappingA");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.IsoIec15434RecordSeparatorMapping);

        token = PerformCalibrationTest("Format06GroupSeparatorMappingB");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.IsoIec15434RecordSeparatorMapping);

        token = PerformCalibrationTest("Format06GroupSeparatorMappingC");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.IsoIec15434RecordSeparatorMapping);

    }

    /// <summary>
    /// Test that the reported calibration data cannot be processed. It does not
    /// include expected delimiters.
    /// </summary>
    [Fact]
    public void ErrorNoDelimiters() {
        var token = PerformCalibrationTest("NoDelimiters");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.NoDelimiters);
    }

    /// <summary>
    /// Test calibration for a malformed barcode that uses every extended ASCII character.
    /// </summary>
    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>")]
    public void ErrorNoTemporaryDelimiterCandidate() {
        /* This test is no longer valid because improvements to the analysis of barcode data means that the malformed barcode
         * will be detected early and the calibrator will not attempt to continue.  The analysis performed on barcode data
         * means that it is now probably impossible for the 'No temporary delimiter candidate' error to be raised.  We will
         * retain the error-generating code in case this proves incorrect.
         * */

        ////var token = this.PerformCalibrationTest("NoTemporaryDelimiterCandidateA");
        ////Assert.Contains(token.Errors, e => e.InformationType == CalibrationInformationType.NoTemporaryDelimiterCandidate);

        ////token = this.PerformCalibrationTest("NoTemporaryDelimiterCandidateB");
        ////Assert.Contains(token.Errors, e => e.InformationType == CalibrationInformationType.NoTemporaryDelimiterCandidate);
    }

    /// <summary>
    /// Test that the reported character {0} is ambiguous. The same character is
    /// reported for multiple dead key sequences representing different expected
    /// characters.
    /// </summary>
    [Fact]
    public void ErrorDeadKeyMultipleKeys() {
        var token = PerformCalibrationTest("DeadKeyMultipleKeys");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.DeadKeyMultipleKeys);
    }

    /// <summary>
    /// Test that the reported character {0} is ambiguous. The same character is
    /// reported for multiple dead key sequences representing different expected
    /// characters.
    /// </summary>
    [Fact]
    public void ErrorDeadKeyMultiMapping() {
        var token = PerformCalibrationTest("DeadKeyMultiMapping");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.DeadKeyMultiMapping);
    }

    /// <summary>
    /// Test if no group separator has been reported, including an ASCII 0.
    /// </summary>
    [Fact]
    public void ErrorNoGroupSeparatorMapping() {
        var token = PerformCalibrationTest("NoGroupSeparatorMapping");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.NoGroupSeparatorMapping);
    }

    /// <summary>
    /// Test that some reported characters are ambiguous. There are multiple keys
    /// for the same character, each representing a different expected character.
    /// However, these characters do not represent characters used in unique pack
    /// identifiers: {0}
    /// </summary>
    [Fact]
    public void WarningMultipleKeysMultipleAsciiCharacters() {
        var token = PerformCalibrationTest("MultipleKeysMultipleNonInvariantCharacters");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.MultipleKeysMultipleNonInvariantCharacters);
    }

    /// <summary>
    /// Test that some key combinations are not recognised. Unique pack identifiers cannot be read reliably. {0}
    /// </summary>
    [Fact]
    public void ErrorSomeDeadKeyCombinationsUnrecognised() {
        var token = PerformCalibrationTest("SomeDeadKeyCombinationsUnrecognised");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.SomeDeadKeyCombinationsUnrecognisedForInvariants);
    }

    /// <summary>
    /// Test that some characters are not recognised by the scanner in its current
    /// configuration. Unique pack identifiers cannot be read reliably. {0}
    /// </summary>
    [Fact]
    public void ErrorSomeCharactersUnrecognised() {
        var token = PerformCalibrationTest("SomeCharactersUnrecognised");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.SomeInvariantCharactersUnrecognised);
    }

    /// <summary>
    /// Test that some characters used in unique pack identifiers cannot be detected. 
    /// </summary>
    [Fact]
    public void ErrorUndetectedUniqueIdentifierCharacters() {
        var token = PerformCalibrationTest("UndetectedUniqueIdentifierCharacters");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.UndetectedInvariantCharacters);
    }

    /// <summary>
    /// Test that too many characters detected. The wrong barcode may have been
    /// scanned.
    /// </summary>
    [Fact]
    public void ErrorTooManyCharactersDetected() {
        var token = PerformCalibrationTest("TooManyCharactersDetectedA");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.TooManyCharactersDetected);

        token = PerformCalibrationTest("TooManyCharactersDetectedB");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.TooManyCharactersDetected);

        token = PerformCalibrationTest("TooManyCharactersDetectedC");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.TooManyCharactersDetected);
    }

    /// <summary>
    /// Test that the reported data is unrecognised. The wrong barcode may have been
    /// scanned.
    /// </summary>
    [Fact]
    public void ErrorUnrecognisedData() {
        var token = PerformCalibrationTest("UnrecognisedData");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.UnrecognisedData);

        token = PerformCalibrationTestWithSegments("UnrecognisedData");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.UnrecognisedData);
    }

    /// <summary>
    /// Test that no calibration data was reported.
    /// </summary>
    [Fact]
    public void ErrorNoCalibrationDataReported() {
        var token = PerformCalibrationTest("NoCalibrationDataReportedA");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.NoCalibrationDataReported);

        token = PerformCalibrationTest("NoCalibrationDataReportedB");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.NoCalibrationDataReported);

        token = PerformCalibrationTestWithSegments("NoCalibrationDataReported");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.NoCalibrationDataReported);
    }

    /// <summary>
    /// Test that no calibration data was provided.
    /// </summary>
    [Fact]
    public void ErrorNoCalibrationTokenProvided() {
        var token = PerformCalibrationTestWithDefaultToken("NoCalibrationDataProvided");
        Assert.Contains(token.Errors, e => e.InformationType == InformationType.NoCalibrationTokenProvided);
    }

    /// <summary>
    /// Test that the Caps Lock button was probably on (or off) during the test.
    /// </summary>
    [Fact]
    public void WarningCapsLockProbablyOn() {
        var token = PerformCalibrationTest("CapsLockProbablyOn");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.CapsLockProbablyOn);

        token = PerformCalibrationTest("CapsLockProbablyOff");
        Assert.DoesNotContain(token.Warnings, e => e.InformationType == InformationType.CapsLockProbablyOn);
    }

    /// <summary>
    /// Test that the barcode scanner and computer keyboard layouts do not correspond with respect to record separators.
    /// </summary>
    [Fact]
    public void WarningNonCorrespondingKeyboardLayoutsRecordSeparator() {
        var token = PerformCalibrationTest("NonCorrespondingKeyboardLayoutsRecordSeparator");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.NonCorrespondingKeyboardLayoutsRecordSeparator);
    }

    /// <summary>
    /// Test that the barcode scanner and computer keyboard layouts do not correspond with respect to group separators.
    /// </summary>
    [Fact]
    public void WarningNonCorrespondingKeyboardLayoutsGroupSeparator() {
        var token = PerformCalibrationTest("NonCorrespondingKeyboardLayoutsGroupSeparator");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.NonCorrespondingKeyboardLayoutsGroupSeparator);
    }

    /// <summary>
    /// Test that the barcode scanner and computer keyboard layouts do not correspond.
    /// </summary>
    [Fact]
    public void WarningNonCorrespondingKeyboardLayouts() {
        var token = PerformCalibrationTest("NonCorrespondingKeyboardLayouts");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.NonCorrespondingKeyboardLayouts);
    }

    /// <summary>
    /// Test that the barcode scanner and computer keyboard layouts do not correspond for characters
    /// used in unique pack identifiers.
    /// </summary>
    [Fact]
    public void WarningNonCorrespondingKeyboardLayoutsForCharacterSet82() {
        var token = PerformCalibrationTest("NonCorrespondingKeyboardLayoutsForCharacterSet82");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.NonCorrespondingKeyboardLayoutsForInvariants);
    }

    /// <summary>
    /// Test that the barcode scanner and computer keyboard layouts do not correspond for
    /// additional ASCII characters that are not used in unique pack identifiers.
    /// </summary>
    [Fact]
    public void WarningNonCorrespondingKeyboardLayoutsAdditionalAsciiCharacters() {
        var token = PerformCalibrationTest("NonCorrespondingKeyboardLayoutsAdditionalAsciiCharacters");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.NonCorrespondingKeyboardLayoutsForNonInvariantCharacters);
    }

    /// <summary>
    /// Test that some reported character sequences are ambiguous. However, these
    /// characters do not represent characters used in unique pack identifiers.
    /// </summary>
    [Fact]
    public void WarningAmbiguousCharacterSequence() {
        var token = PerformCalibrationTest("AmbiguousCharacterSequence");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.NonInvariantCharacterSequence);
    }

    /// <summary>
    /// Test that some reported characters are ambiguous. However, these characters
    /// do not represent characters used in unique pack identifiers.
    /// </summary>
    [Fact]
    public void WarningDeadKeyMultiMappingAdditionalAsciiCharacters() {
        var token = PerformCalibrationTest("DeadKeyMultiMappingAdditionalAsciiCharactersA");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.DeadKeyMultiMappingNonInvariantCharacters);

        token = PerformCalibrationTest("DeadKeyMultiMappingAdditionalAsciiCharactersB");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.DeadKeyMultiMappingNonInvariantCharacters);
    }

    /// <summary>
    /// Test that AIM Identifiers cannot be recognized. There are multiple keys for
    /// the AIM flag character, each representing a different expected character.
    /// </summary>
    [Fact]
    public void WarningMultipleKeysAimFlagCharacter() {
        var token = PerformCalibrationTest("MultipleKeysAimFlagCharacter");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.MultipleKeysAimFlagCharacter);
    }

    /// <summary>
    /// Test that the reported character is ambiguous. There are multiple keys for
    /// the same character, each representing a different expected character.
    /// However, at most, only one of the expected characters is used in unique pack
    /// identifiers.
    /// </summary>
    [Fact]
    public void WarningMultipleKeysAdditionalAsciiCharacters() {
        var token = PerformCalibrationTest("MultipleKeysAdditionalAsciiCharactersA");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.MultipleKeysNonInvariantCharacters);

        token = PerformCalibrationTest("MultipleKeysAdditionalAsciiCharactersB");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.MultipleKeysNonInvariantCharacters);
    }

    /// <summary>
    /// Test that the scanner is not transmitting an end-of-line character sequence (e.g. a carriage return).
    /// </summary>
    [Fact]
    public void WarningEndOfLineNotTransmitted() {
        var token = PerformCalibrationTest("EndOfLineNotTransmitted");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.EndOfLineNotTransmitted);
    }

    /// <summary>
    /// Test that the scanner is transmitting a suffix.
    /// </summary>
    [Fact]
    public void WarningSuffixTransmitted() {
        var token = PerformCalibrationTest("SuffixTransmitted");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.SuffixTransmitted);
    }

    /// <summary>
    /// Test that the scanner is transmitting a prefix.
    /// </summary>
    [Fact]
    public void WarningPrefixTransmitted() {
        var token = PerformCalibrationTest("PrefixTransmittedA");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.PrefixTransmitted);

        token = PerformCalibrationTest("PrefixTransmittedB");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.PrefixTransmitted);

        token = PerformCalibrationTest("PrefixTransmittedC");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.PrefixTransmitted);

        token = PerformCalibrationTest("PrefixTransmittedD");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.PrefixTransmitted);

        token = PerformCalibrationTest("PrefixTransmittedE");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.PrefixTransmitted);

        token = PerformCalibrationTest("PrefixTransmittedF");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.PrefixTransmitted);
    }

    /// <summary>
    /// Test that the AIM Identifier cannot be recognised.
    /// </summary>
    [Fact]
    public void WarningAimNotRecognised() {
        var token = PerformCalibrationTest("AimNotRecognised");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.AimNotRecognised);
    }

    /// <summary>
    /// Test that the scanner is not transmitting an AIM Identifier.
    /// </summary>
    [Fact]
    public void WarningAimNotTransmitted() {
        var token = PerformCalibrationTest("AimNotTransmittedA");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.AimNotTransmitted);

        token = PerformCalibrationTest("AimNotTransmittedB");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.AimNotTransmitted);
    }

    /// <summary>
    /// Test that some combinations of characters are not recognised. However, these character
    /// combinations are not used in unique pack identifiers.
    /// </summary>
    [Fact]
    public void WarningSomeAdditionalCharacterCombinationsUnrecognised() {
        var token = PerformCalibrationTest("SomeNonInvariantCharacterCombinationsUnrecognised");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.SomeNonInvariantCharacterCombinationsUnrecognised);
    }

    /// <summary>
    /// Test that some characters are not recognised. However, these characters are not used in unique pack identifiers.
    /// </summary>
    [Fact]
    public void WarningSomeAdditionalCharactersUnrecognised() {
        var token = PerformCalibrationTest("SomeAdditionalCharactersUnrecognisedA");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.SomeNonInvariantCharactersUnrecognised);

        token = PerformCalibrationTest("SomeAdditionalCharactersUnrecognisedB");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.SomeNonInvariantCharactersUnrecognised);
    }

    /// <summary>
    /// Test that some characters cannot be detected. However, these characters are not used in unique pack identifiers.
    /// </summary>
    [Fact]
    public void WarningSomeCharactersUnreported() {
        var token = PerformCalibrationTest("SomeNonInvariantCharactersUnreported");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.SomeNonInvariantCharactersUnreported);
    }

    /// <summary>
    /// Test that non-correspondence for AIM flag character is reported.
    /// </summary>
    [Fact]
    public void WarningNonCorrespondingKeyboardLayoutsForAimIdentifier() {
        var token = PerformCalibrationTest("NonCorrespondingKeyboardLayoutsForAimIdentifier1");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.NonCorrespondingKeyboardLayoutsForAimIdentifier);
        token = PerformCalibrationTest("NonCorrespondingKeyboardLayoutsForAimIdentifier2");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.NonCorrespondingKeyboardLayoutsForAimIdentifier);
    }

    /// <summary>
    /// Test that the barcode scanner/computer combination supports Record Separator characters.
    /// </summary>
    [Fact]
    public void InformationKeyboardScript() {
        var token = PerformCalibrationTest("KeyboardScriptLatin");
        Assert.Equal("Latin", token.ExtendedData?.KeyboardScript);

        token = PerformCalibrationTest("KeyboardScriptGreek");
        Assert.Equal("Greek", token.ExtendedData?.KeyboardScript);

        token = PerformCalibrationTest("KeyboardScriptCyrillic");
        Assert.Equal("Cyrillic", token.ExtendedData?.KeyboardScript);
    }

    /// <summary>
    /// Test that the barcode scanner/computer combination supports Record Separator characters.
    /// </summary>
    [Fact]
    public void InformationRecordSeparatorSupported() {
        var token = PerformCalibrationTest("RecordSeparatorSupported");
        Assert.Contains(token.Information, e => e.InformationType == InformationType.RecordSeparatorSupported);
    }

    /// <summary>
    /// Test that the barcode scanner/computer combination supports Group Separator characters.
    /// </summary>
    [Fact]
    public void InformationGroupSeparatorSupported() {
        var token = PerformCalibrationTest("GroupSeparatorSupported");
        Assert.Contains(token.Information, e => e.InformationType == InformationType.GroupSeparatorSupported);
    }


    /// <summary>
    /// Test that the scanner is transmitting an end-of-line character sequence.
    /// </summary>
    [Fact]
    public void InformationEndOfLineTransmitted() {
        var token = PerformCalibrationTest("EndOfLineTransmitted");
        Assert.Contains(token.Information, e => e.InformationType == InformationType.EndOfLineTransmitted);
    }

    /// <summary>
    /// Test that the scanner is transmitting an AIM identifier.
    /// </summary>
    [Fact]
    public void InformationAimTransmitted() {
        var token = PerformCalibrationTest("AimTransmitted");
        Assert.Contains(token.Information, e => e.InformationType == InformationType.AimTransmitted);
    }

    /// <summary>
    /// Test that AIM identifiers are supported.
    /// </summary>
    [Fact]
    public void InformationAimSupported() {
        var token = PerformCalibrationTest("AimSupported");
        Assert.Contains(token.Information, e => e.InformationType == InformationType.AimSupported);
    }

    /// <summary>
    /// Test that scanner Caps Lock compensation is detected.
    /// </summary>
    [Fact]
    public void InformationScannerMayCompensateForCapsLock() {
        var token = PerformCalibrationTest("ScannerMayCompensateForCapsLock", capsLock: true);
        Assert.Contains(token.Information, e => e.InformationType == InformationType.ScannerMayCompensateForCapsLock);
    }

    /// <summary>
    /// Test that Caps Lock warning is issued.
    /// </summary>
    [Fact]
    public void WarningCapsLockOn() {
        var token = PerformCalibrationTest("CapsLockOn", capsLock: true);
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.CapsLockOn);
    }

    /// <summary>
    /// Test that conversion to upper case is reported.
    /// </summary>
    [Fact]
    public void WarningScannerMayConvertToUpperCase() {
        var token = PerformCalibrationTest("ScannerMayConvertToUpperCase");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.ScannerMayConvertToUpperCase);
    }

    /// <summary>
    /// Test that conversion to lower case is reported.
    /// </summary>
    [Fact]
    public void WarningScannerMayConvertToLowerCase() {
        var token = PerformCalibrationTest("ScannerMayConvertToLowerCase");
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.ScannerMayConvertToLowerCase);
    }

    /// <summary>
    /// Test that case inversion is reported.
    /// </summary>
    [Fact]
    public void WarningScannerMayInvertCase() {
        var token = PerformCalibrationTest("ScannerMayInvertCase", capsLock: false);
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.ScannerMayInvertCase);
        token = PerformCalibrationTest("ScannerMayInvertCase", platform: SupportedPlatform.Macintosh);
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.ScannerMayInvertCase);
    }

    /// <summary>
    /// Test that slow scanner performance is reported.
    /// </summary>
    [Fact]
    public void WarningSubOptimalScannerKeyboardPerformance() {
        var token = PerformCalibrationTest("SubOptimalScannerKeyboardPerformance", dataEntryTimeSpan: new TimeSpan(0, 0, 0, 0, 500));
        Assert.DoesNotContain(token.Warnings, e => e.InformationType == InformationType.SubOptimalScannerKeyboardPerformance);
        token = PerformCalibrationTest("SubOptimalScannerKeyboardPerformance", dataEntryTimeSpan: new TimeSpan(0, 0, 0, 1, 500));
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.SubOptimalScannerKeyboardPerformance);
        token = PerformCalibrationTest("SubOptimalScannerKeyboardPerformance", dataEntryTimeSpan: new TimeSpan(0, 0, 0, 3));
        Assert.Contains(token.Warnings, e => e.InformationType == InformationType.SubOptimalScannerKeyboardPerformance);
    }

    /// <summary>
    /// Test that AIM identifiers are supported.
    /// </summary>
    [Fact]
    public void ChainedSequence() {
        var token = PerformCalibrationTest("ChainedSequence");
        Assert.Contains(token.Information, e => e.InformationType == InformationType.AimSupported);
    }

    /// <summary>
    /// Returns test data for testing calibration for a scanner configured as a United States keyboard and
    /// computer keyboard layouts for each European keyboard defined in Windows.
    /// </summary>
    /// <returns>A dictionary of test data.</returns>
    private static Dictionary<string, Dictionary<string, IList<string>>> UnitedStatesTestData() {
        var unitedStatesTestData = new Dictionary<string, Dictionary<string, IList<string>>>
                                   {
                                       {
                                           "MultipleKeys",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   MultipleKeys,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "MultipleSequences",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   MultipleSequences,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "MultipleSequencesForScannerDeadKey",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   MultipleSequencesForScannerDeadKey,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "GroupSeparatorMappingA",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   GroupSeparatorMappingA,
                                                   new List<string>
                                                   {
                                                       GroupSeparatorMappingADeadKey1
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "GroupSeparatorMappingB",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   GroupSeparatorMappingB,
                                                   new List<string>
                                                   {
                                                       GroupSeparatorMappingBDeadKey1
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "GroupSeparatorMappingC",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   GroupSeparatorMappingC,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "GroupSeparatorMappingD",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   GroupSeparatorMappingD,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "ControlCharacterMappingA",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   ControlCharacterMappingA,
                                                   new List<string>
                                                   {
                                                       ControlCharacterMappingADeadKey1
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "ControlCharacterMappingB",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   ControlCharacterMappingB,
                                                   new List<string>
                                                   {
                                                       ControlCharacterMappingBDeadKey1
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "ControlCharacterMappingC",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   ControlCharacterMappingC,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "ControlCharacterMappingD",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   ControlCharacterMappingD,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "ControlCharacterMappingE",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   ControlCharacterMappingE,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "Ligature Control",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   LigatureControlMapping,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "Format0506NotRecognisedA",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   Format0506NotRecognisedA,
                                                   new List<string>
                                                   {
                                                       Format0506NotRecognisedADeadKey1
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Format0506NotRecognisedB",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   Format0506NotRecognisedB,
                                                   new List<string>
                                                   {
                                                       Format0506NotRecognisedADeadKey2
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Format0506NotRecognisedC",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   Format0506NotRecognisedC,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "Format0506NotRecognisedD",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   Format0506NotRecognisedD,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "Format0506NotReliablyRecognised1",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   Format0506NotReliablyRecognised1,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "Format06GroupSeparatorMappingA",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   Format06GroupSeparatorMappingA,
                                                   new List<string>
                                                   {
                                                       Format06GroupSeparatorMappingADeadKey1
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Format06GroupSeparatorMappingB",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   Format06GroupSeparatorMappingB,
                                                   new List<string>
                                                   {
                                                       Format06GroupSeparatorMappingBDeadKey1
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "Format06GroupSeparatorMappingC",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   Format06GroupSeparatorMappingC,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "IncompatibleScannerDeadKey",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   IncompatibleScannerDeadKey,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "NoDelimiters",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   NoDelimiters,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "NoTemporaryDelimiterCandidateA",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   NoTemporaryDelimiterCandidateA,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "NoTemporaryDelimiterCandidateB",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   NoTemporaryDelimiterCandidateB,
                                                   new List<string>
                                                   {
                                                       NoTemporaryDelimiterCandidateBDeadKey1
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "DeadKeyMultipleKeys",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   DeadKeyMultipleKeys,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "DeadKeyMultiMapping",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   DeadKeyMultiMapping,
                                                   new List<string>
                                                   {
                                                       DeadKeyMultiMappingDeadKey1
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "NoGroupSeparatorMapping",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   NoGroupSeparatorMapping,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "MultipleKeysMultipleNonInvariantCharacters",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   MultipleKeysMultipleAsciiCharacters,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "SomeDeadKeyCombinationsUnrecognised",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   SomeDeadKeyCombinationsUnrecognised,
                                                   new List<string>
                                                   {
                                                       SomeDeadKeyCombinationsUnrecognisedDeadKey1,
                                                       SomeDeadKeyCombinationsUnrecognisedDeadKey2
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "SomeCharactersUnrecognised",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   SomeCharactersUnrecognised,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "UndetectedUniqueIdentifierCharacters",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   UndetectedUniqueIdentifierCharacters,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "TooManyCharactersDetectedA",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   TooManyCharactersDetectedA,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "TooManyCharactersDetectedB",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   TooManyCharactersDetectedB,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "TooManyCharactersDetectedC",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   TooManyCharactersDetectedC,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "UnrecognisedData",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   UnrecognisedData,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "NoCalibrationDataProvided",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   NoCalibrationDataProvided,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "NoCalibrationDataReportedA",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   NoCalibrationDataReportedA,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "NoCalibrationDataReportedB",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   NoCalibrationDataReportedB,
                                                   new List<string>
                                                   {
                                                       NoCalibrationDataReportedCDeadKey1
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "NonCorrespondingKeyboardLayoutsRecordSeparator",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   NonCorrespondingKeyboardLayoutsRecordSeparator,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "CapsLockProbablyOff",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   CapsLockProbablyOff,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "CapsLockProbablyOn",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   CapsLockProbablyOn,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "NonCorrespondingKeyboardLayoutsGroupSeparator",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   NonCorrespondingKeyboardLayoutsGroupSeparator,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "NonCorrespondingKeyboardLayouts",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   NonCorrespondingKeyboardLayouts,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "NonCorrespondingKeyboardLayoutsForCharacterSet82",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   NonCorrespondingKeyboardLayoutsForCharacterSet82,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "NonCorrespondingKeyboardLayoutsAdditionalAsciiCharacters",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   NonCorrespondingKeyboardLayoutsAdditionalAsciiCharacters,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "IncompatibleScannerDeadKeyAdditionalAsciiCharacters",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   IncompatibleScannerDeadKeyAdditionalAsciiCharacters,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "AmbiguousCharacterSequence",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   AmbiguousCharacterSequence,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "DeadKeyMultiMappingAdditionalAsciiCharactersA",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   DeadKeyMultiMappingAdditionalAsciiCharactersA,
                                                   new List<string>
                                                   {
                                                       DeadKeyMultiMappingAdditionalAsciiCharactersADeadKey1
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "DeadKeyMultiMappingAdditionalAsciiCharactersB",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   DeadKeyMultiMappingAdditionalAsciiCharactersB,
                                                   new List<string>
                                                   {
                                                       DeadKeyMultiMappingAdditionalAsciiCharactersBDeadKey1,
                                                       DeadKeyMultiMappingAdditionalAsciiCharactersBDeadKey2
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "MultipleKeysAimFlagCharacter",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   MultipleKeysAimFlagCharacter,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "MultipleKeysAdditionalAsciiCharactersA",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   MultipleKeysAdditionalAsciiCharactersA,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "MultipleKeysAdditionalAsciiCharactersB",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   MultipleKeysAdditionalAsciiCharactersB,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "EndOfLineNotTransmitted",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   EndOfLineNotTransmitted,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "SuffixTransmitted",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   SuffixTransmitted,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "PrefixTransmittedA",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   PrefixTransmittedA,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "PrefixTransmittedB",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   PrefixTransmittedB,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "PrefixTransmittedC",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   PrefixTransmittedC,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "PrefixTransmittedD",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   PrefixTransmittedD,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "PrefixTransmittedE",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   PrefixTransmittedE,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "PrefixTransmittedF",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   PrefixTransmittedF,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "AimNotRecognised",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   AimNotRecognised,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "AimNotTransmittedA",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   AimNotTransmittedA,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "AimNotTransmittedB",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   AimNotTransmittedB,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "SomeNonInvariantCharacterCombinationsUnrecognised",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   SomeAdditionalCharacterCombinationsUnrecognised,
                                                   new List<string>
                                                   {
                                                       SomeAdditionalCharacterCombinationsUnrecognisedDeadKey1,
                                                       SomeAdditionalCharacterCombinationsUnrecognisedDeadKey2
                                                   }
                                               }
                                           }
                                       },
                                       {
                                           "SomeAdditionalCharactersUnrecognisedA",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   SomeAdditionalCharactersUnrecognisedA,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "SomeAdditionalCharactersUnrecognisedB",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   SomeAdditionalCharactersUnrecognisedB,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "SomeNonInvariantCharactersUnreported",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   SomeCharactersUnreported,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "NonCorrespondingKeyboardLayoutsForAimIdentifier1",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   NonCorrespondingKeyboardLayoutsForAimIdentifier1,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "NonCorrespondingKeyboardLayoutsForAimIdentifier2",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   NonCorrespondingKeyboardLayoutsForAimIdentifier2,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "KeyboardScriptCyrillic",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   KeyboardScriptCyrillic,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "KeyboardScriptGreek",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   KeyboardScriptGreek,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "KeyboardScriptLatin",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   KeyboardScriptLatin,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "RecordSeparatorSupported",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   RecordSeparatorSupported,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "GroupSeparatorSupported",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   GroupSeparatorSupported,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "EndOfLineTransmitted",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   EndOfLineTransmitted,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "AimTransmitted",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   AimTransmitted,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "AimSupported",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   AimSupported,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "ScannerMayCompensateForCapsLock",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   ScannerMayCompensateForCapsLock,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "CapsLockOn",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   CapsLockOn,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "ScannerMayConvertToUpperCase",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   ScannerMayConvertToUpperCase,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "ScannerMayConvertToLowerCase",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   ScannerMayConvertToLowerCase,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "ScannerMayInvertCase",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   ScannerMayInvertCase,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "SubOptimalScannerKeyboardPerformance",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   SubOptimalScannerKeyboardPerformance,
                                                   new List<string>()
                                               }
                                           }
                                       },
                                       {
                                           "ChainedSequence",
                                           new Dictionary<string, IList<string>>
                                           {
                                               {
                                                   ChainedSequenceData,
                                                   new List<string>()
                                               }
                                           }
                                       }
                                   };

        return unitedStatesTestData;
    }

    /// <summary>
    /// Returns test data for testing calibration for a scanner configured as a United States keyboard and
    /// computer keyboard layouts for each European keyboard defined in Windows.  Supports segmented bar codes.
    /// </summary>
    /// <returns>A dictionary of test data.</returns>
    private static Dictionary<string, Dictionary<IList<string>, Dictionary<string, IList<string>>>> UnitedStatesSegmentedTestData() {
        var unitedStatesTestData =
            new Dictionary<string, Dictionary<IList<string>, Dictionary<string, IList<string>>>>
            {
                {
                    "NoCalibrationDataReported",
                    new Dictionary<IList<string>, Dictionary<string, IList<string>>>
                    {
                        {
                            new List<string>
                            {
                                NoCalibrationDataReportedD1,
                                NoCalibrationDataReportedD2,
                                NoCalibrationDataReportedD3,
                                NoCalibrationDataReportedD4,
                                NoCalibrationDataReportedD5,
                                NoCalibrationDataReportedD6
                            },
                            new Dictionary<string, IList<string>>
                            {
                                {
                                    "NoCalibrationDataReportedDeadKey",
                                    new List<string>
                                    {
                                        NoCalibrationDataReportedDeadKey11,
                                        NoCalibrationDataReportedDeadKey12,
                                        NoCalibrationDataReportedDeadKey13,
                                        NoCalibrationDataReportedDeadKey14,
                                        NoCalibrationDataReportedDeadKey15
                                    }
                                }
                            }
                        }
                    }
                },
                {
                    "UnrecognisedData",
                    new Dictionary<IList<string>, Dictionary<string, IList<string>>>
                    {
                        {
                            new List<string>
                            {
                                UnrecognisedDataD1,
                                UnrecognisedDataD2,
                                UnrecognisedDataD3,
                                UnrecognisedDataD4,
                                UnrecognisedDataD5,
                                UnrecognisedDataD6,
                                UnrecognisedDataD7
                            },
                            new Dictionary<string, IList<string>>
                            {
                                {
                                    "UnrecognisedDataDeadKey",
                                    new List<string>
                                    {
                                        UnrecognisedDataDeadKey11,
                                        UnrecognisedDataDeadKey12,
                                        UnrecognisedDataDeadKey13,
                                        UnrecognisedDataDeadKey14,
                                        UnrecognisedDataDeadKey15,
                                        UnrecognisedDataDeadKey16
                                    }
                                }
                            }
                        }
                    }
                }
            };

        return unitedStatesTestData;
    }

    /// <summary>
    /// Get a string containing all extended ASCII characters.
    /// </summary>
    /// <returns>A string containing all extended ASCII characters.</returns>
    private static string GetAllExtendedAscii() {
        var outCharacters = new StringBuilder();

        for (var idx = 127; idx < 256; idx++) {
            outCharacters.Append((char)idx);
        }

        return outCharacters.ToString();
    }

    /// <summary>
    /// Get a string containing all extended ASCII characters preceded by a literal character.
    /// </summary>
    /// <param name="literal">
    /// The literal character.
    /// </param>
    /// <returns>
    /// A string containing all extended ASCII characters preceded by a literal character.
    /// </returns>
    private static string GetAllExtendedAsciiDeadKey(char literal) {
        var outCharacters = new StringBuilder();

        for (var idx = 127; idx < 256; idx++) {
            outCharacters.Append(literal);
            outCharacters.Append((char)idx);
        }

        return outCharacters.ToString();
    }

    /// <summary>
    /// Performs a calibration test.
    /// </summary>
    /// <param name="layoutName">The name of the computer keyboard layout</param>
    /// <param name="capsLock">Indicates if Caps Lock is switched on.</param>
    /// <param name="platform">The platform on which the system resides.</param>
    /// <param name="dataEntryTimeSpan">The time span specifying how long it took from the start of the scan to submitting the data.</param>
    /// <returns>A calibration token.</returns>
    private static Token PerformCalibrationTest(
        string layoutName,
        bool? capsLock = null,
        SupportedPlatform platform = SupportedPlatform.Windows,
        TimeSpan dataEntryTimeSpan = default) {
        Debug.WriteLine(layoutName);

        var computerKeyboardLayout = UnitedStatesTestData()[layoutName];
        var calibrator = new Calibrator();
        var loopCount = -1;
        Token currentToken = default;

        var recognisedDataElements = new List<RecognisedDataElement>
        {
            new(Syntax.Gs1ApplicationIdentifiers, "01"),
            new(Syntax.Gs1ApplicationIdentifiers, "10"),
            new(Syntax.Gs1ApplicationIdentifiers, "17"),
            new(Syntax.Gs1ApplicationIdentifiers, "21"),
            new(Syntax.Gs1ApplicationIdentifiers, "710"),
            new(Syntax.Gs1ApplicationIdentifiers, "711"),
            new(Syntax.Gs1ApplicationIdentifiers, "712"),
            new(Syntax.Gs1ApplicationIdentifiers, "714"),
            new(Syntax.AscMhDataIdentifiers, "9N"),
            new(Syntax.AscMhDataIdentifiers, "1T"),
            new(Syntax.AscMhDataIdentifiers, "D"),
            new(Syntax.AscMhDataIdentifiers, "S")
        };

        calibrator.RecognisedDataElements = recognisedDataElements;

        foreach (var token in calibrator.CalibrationTokens()) {
            var baseLine = computerKeyboardLayout.Keys.First();
            currentToken = token;

            if (loopCount < 0) {
                currentToken = calibrator.Calibrate(ConvertToCharacterValues(baseLine), currentToken, capsLock, platform, dataEntryTimeSpan);
                loopCount++;
            }
            else {
                if (loopCount < computerKeyboardLayout[baseLine].Count) {
                    currentToken = calibrator.Calibrate(
                        ConvertToCharacterValues(computerKeyboardLayout[baseLine][loopCount++]),
                        currentToken,
                        capsLock,
                        platform,
                        dataEntryTimeSpan);
                }
            }

            foreach (var error in currentToken.Errors) {
                Debug.WriteLine(error.Description);
            }
        }

        Trace.WriteLine(
            $"private const string {layoutName.Replace(" ", "").Replace("(", "").Replace(")", "")}Calibration = " +
            ToLiteral($"\"{calibrator.CalibrationData}\";"));

        return currentToken;

        static string ToLiteral(string input) {
            using var writer = new StringWriter();
            using var provider = CodeDomProvider.CreateProvider("CSharp");
            provider.GenerateCodeFromExpression(new System.CodeDom.CodePrimitiveExpression(input), writer, null!);
            return writer.ToString();
        }
    }

    /// <summary>
    /// Performs a calibration test incorrectly with a default token.
    /// </summary>
    /// <param name="layoutName">The name of the computer keyboard layout</param>
    /// <returns>A calibration token.</returns>
    private static Token PerformCalibrationTestWithDefaultToken(string layoutName) {
        Debug.WriteLine(layoutName);

        var computerKeyboardLayout = UnitedStatesTestData()[layoutName];
        var calibrator = new Calibrator();
        var loopCount = -1;
        Token currentToken = default;

        foreach (var token in calibrator.CalibrationTokens()) {
            var baseLine = computerKeyboardLayout.Keys.First();
            currentToken = token;

            if (loopCount < 0) {
                currentToken = calibrator.Calibrate(ConvertToCharacterValues(baseLine), new Token());
                loopCount++;
            }
            else {
                if (loopCount < computerKeyboardLayout[baseLine].Count) {
                    currentToken = calibrator.Calibrate(
                        ConvertToCharacterValues(computerKeyboardLayout[baseLine][loopCount++]),
                        new Token());
                }
            }

            foreach (var error in currentToken.Errors) {
                Debug.WriteLine(error.Description);
            }
        }

        Trace.WriteLine(
            $"private const string {layoutName.Replace(" ", "").Replace("(", "").Replace(")", "")}Calibration = " +
            ToLiteral($"\"{calibrator.CalibrationData}\";"));

        return currentToken;

        static string ToLiteral(string input) {
            using var writer = new StringWriter();
            using var provider = CodeDomProvider.CreateProvider("CSharp");
            provider.GenerateCodeFromExpression(new System.CodeDom.CodePrimitiveExpression(input), writer, null!);
            return writer.ToString();
        }
    }

    /// <summary>
    /// Performs a calibration test incorrectly with a default token.
    /// </summary>
    /// <param name="layoutName">The name of the computer keyboard layout</param>
    /// <returns>A calibration token.</returns>
    private static Token PerformCalibrationTestWithSegments(string layoutName) {
        Debug.WriteLine(layoutName);

        var computerKeyboardLayout = UnitedStatesSegmentedTestData()[layoutName];
        var calibrator = new Calibrator();
        var currentDeadKey = string.Empty;
        var deadKey = string.Empty;
        var keyIndex = 0;
        Token currentToken = default;

        foreach (var token in calibrator.CalibrationTokens(1F, Size.Dm26X26)) {
            Debug.WriteLine(token.Data?.BarcodeData);
            var deadKeyData = computerKeyboardLayout[computerKeyboardLayout.Keys.First()];
            string data;

            if (token.Data?.Key == string.Empty) {
                // This is the baseline calibration
                data = computerKeyboardLayout.Keys.First()[token.Data.SmallBarcodeSequenceIndex - 1];
            }
            else {
                // This is a dead key calibration
                if (currentDeadKey == string.Empty) {
                    currentDeadKey = deadKeyData.Keys.First();
                    deadKey = token.Data?.Key;
                    keyIndex++;
                }
                else if (deadKey != token.Data?.Key && keyIndex < deadKeyData.Keys.Count) {
                    currentDeadKey = deadKeyData.Keys.ElementAt(keyIndex++);
                    deadKey = token.Data?.Key;
                }

                data = deadKeyData[currentDeadKey][(token.Data?.SmallBarcodeSequenceIndex ?? 0) - 1];
            }

            currentToken = calibrator.Calibrate(data, token);

            foreach (var error in currentToken.Errors) {
                Debug.WriteLine(error.Description);
            }
        }

        Trace.WriteLine(
            $"\r\rprivate const string {layoutName.Replace(" ", "").Replace("(", "").Replace(")", "")}Calibration = " +
            ToLiteral($"\"{calibrator.CalibrationData}\";"));

        return currentToken;

        static string ToLiteral(string input) {
            using var writer = new StringWriter();
            using var provider = CodeDomProvider.CreateProvider("CSharp");
            provider.GenerateCodeFromExpression(new System.CodeDom.CodePrimitiveExpression(input), writer, null!);
            return writer.ToString();
        }
    }

    /// <summary>
    /// Return the character input for the baseline calibration barcode using a scanner
    /// configured for a US keyboard and computer with US keyboard layout. 
    /// </summary>
    /// <returns>
    /// The <see cref="string"/> for scanning the baseline calibration barcode.
    /// The scanner is configured for a US keyboard.
    /// The computer is configured for a US keyboard.
    /// </returns>
    private static string BaselineCalibrationUsUs() {
        var testString =
            "  ! \" % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? A B C D E F G H I J K L M N O P Q R S T U V W X Y Z _ a b c d e f g h i j k l m n o p q r s t u v w x y z   # $ @ [ \\ ] ^ ` { | } ~    "
          + (char)29;
        return testString + "    \x001C    \x001E    \x001F    ";
    }

    /// <summary>
    /// Convert an input string to a list of character values.
    /// </summary>
    /// <param name="input">The string of characters to be converted.</param>
    /// <returns>A comma-separated value list of character values.</returns>
    private static int[] ConvertToCharacterValues(string input) {
        if (string.IsNullOrWhiteSpace(input)) {
            return [];
        }

        var outputBuilder = new int[input.Length];

        for (var idx = 0; idx < input.Length; idx++) {
            outputBuilder[idx] = input[idx];
        }

        return outputBuilder;
    }
}