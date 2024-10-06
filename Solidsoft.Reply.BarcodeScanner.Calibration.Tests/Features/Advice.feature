Feature: Advice

Basic tests for general advice provided by the Calibrator library.
All tests assume a computer configured for a standard USA keyboard.

Scenario: System reads Invariant Characters reliably

	Given the baseline input is for The United States
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
		And the advice should contain no other advice items

Scenario: No FS character reported

	Given the baseline input is for The United States with no FS
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
	    And the advice should contain an advice item for CannotReadEdiCharacters
	    And the advice should contain an advice item for CannotReadAscii28Characters
		And the advice should contain no other advice items

Scenario: No US character reported

	Given the baseline input is for The United States with no US
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
	    And the advice should contain an advice item for CannotReadEdiCharacters
	    And the advice should contain an advice item for CannotReadAscii31Characters
		And the advice should contain no other advice items

Scenario: No EOT character reported

	Given the baseline input is for The United States with no EOT
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
	    And the advice should contain an advice item for CannotReadAscii04Characters
		And the advice should contain no other advice items

Scenario: Null FS character reported

	Given the baseline input is for The United States with null FS
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
	    And the advice should contain an advice item for CannotReadEdiCharacters
	    And the advice should contain an advice item for CannotReadAscii28Characters
		And the advice should contain no other advice items

Scenario: Null US character reported

	Given the baseline input is for The United States with null US
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
	    And the advice should contain an advice item for CannotReadEdiCharacters
	    And the advice should contain an advice item for CannotReadAscii31Characters
		And the advice should contain no other advice items

Scenario: Null EOT character reported

	Given the baseline input is for The United States with null EOT
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
	    And the advice should contain an advice item for CannotReadAscii04Characters
		And the advice should contain no other advice items

Scenario: FS character reported as different character - agnostic

	Given the baseline input is for The United States with FS as different character
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
	    And the advice should contain an advice item for MayNotReadEdiCharactersReliably
	    And the advice should contain an advice item for MayNotReadAscii28Characters
		And the advice should contain no other advice items

Scenario: US character reported as different character - agnostic

	Given the baseline input is for The United States with US as different character
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
	    And the advice should contain an advice item for MayNotReadEdiCharactersReliably
	    And the advice should contain an advice item for MayNotReadAscii31Characters
		And the advice should contain no other advice items

Scenario: EOT character reported as different character - agnostic

	Given the baseline input is for The United States with EOT as different character
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
	    And the advice should contain an advice item for MayNotReadAscii04Characters
		And the advice should contain no other advice items

Scenario: FS character reported as different character - calibration

	Given the baseline input is for The United States with FS as different character
	When the baseline input to submitted to a calibration calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
		And the advice should contain no other advice items

Scenario: US character reported as different character - calibration

	Given the baseline input is for The United States with US as different character
	When the baseline input to submitted to a calibration calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
		And the advice should contain no other advice items

Scenario: EOT character reported as different character - calibration

	Given the baseline input is for The United States with EOT as different character
	When the baseline input to submitted to a calibration calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
		And the advice should contain no other advice items

Scenario: FS character reported as different character - no calibration

	Given the baseline input is for The United States with FS as different character
	When the baseline input to submitted to a no calibration calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
	    And the advice should contain an advice item for CannotReadEdiCharacters
	    And the advice should contain an advice item for CannotReadAscii28Characters
		And the advice should contain no other advice items

Scenario: US character reported as different character - no calibration

	Given the baseline input is for The United States with US as different character
	When the baseline input to submitted to a no calibration calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
	    And the advice should contain an advice item for CannotReadEdiCharacters
	    And the advice should contain an advice item for CannotReadAscii31Characters
		And the advice should contain no other advice items

Scenario: EOT character reported as different character - no calibration

	Given the baseline input is for The United States with EOT as different character
	When the baseline input to submitted to a no calibration calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
	    And the advice should contain an advice item for CannotReadAscii04Characters
		And the advice should contain no other advice items

Scenario: FS character reported as ambiguous invariant character

	Given the baseline input is for The United States with FS as ambiguous invariant character
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for CannotReadBarcodesReliably
		And the advice should contain no other advice items

Scenario: US character reported as ambiguous invariant character

	Given the baseline input is for The United States with US as ambiguous invariant character
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for CannotReadBarcodesReliably
		And the advice should contain no other advice items

Scenario: EOT character reported as ambiguous invariant character

	Given the baseline input is for The United States with EOT as ambiguous invariant character
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for CannotReadBarcodesReliably
		And the advice should contain no other advice items

Scenario: FS character reported as ambiguous non-invariant character

	Given the baseline input is for The United States with FS as ambiguous non-invariant character
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
	    And the advice should contain an advice item for MayNotReadEdiCharactersReliably
	    And the advice should contain an advice item for MayNotReadAscii28Characters
		And the advice should contain no other advice items

Scenario: US character reported as ambiguous non-invariant character

	Given the baseline input is for The United States with US as ambiguous non-invariant character
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
	    And the advice should contain an advice item for MayNotReadEdiCharactersReliably
	    And the advice should contain an advice item for MayNotReadAscii31Characters
		And the advice should contain no other advice items

Scenario: EOT character reported as ambiguous non-invariant character

	Given the baseline input is for The United States with EOT as ambiguous non-invariant character
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
	    And the advice should contain an advice item for MayNotReadAscii04Characters
		And the advice should contain no other advice items

Scenario: FS character reported as AIM flag character

	Given the baseline input is for The United States with FS as AIM flag character
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
	    And the advice should contain an advice item for MayNotReadEdiCharactersReliably
	    And the advice should contain an advice item for MayNotReadAscii28Characters
		And the advice should contain no other advice items

Scenario: US character reported as AIM flag character

	Given the baseline input is for The United States with US as AIM flag character
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
	    And the advice should contain an advice item for MayNotReadEdiCharactersReliably
	    And the advice should contain an advice item for MayNotReadAscii31Characters
		And the advice should contain no other advice items

Scenario: EOT character reported as AIM flag character

	Given the baseline input is for The United States with EOT as AIM flag character
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
	    And the advice should contain an advice item for MayNotReadAscii04Characters
		And the advice should contain no other advice items

Scenario: FS character reported as dead key character

	Given the baseline input is for The United States with FS as dead key character
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
		And the advice should contain no other advice items

Scenario: US character reported as dead key character

	Given the baseline input is for The United States with US as dead key character
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
		And the advice should contain no other advice items

Scenario: EOT character reported as dead key character

	Given the baseline input is for The United States with EOT as dead key character
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
		And the advice should contain no other advice items

Scenario: FS character reported as ligature

	Given the baseline input is for The United States with FS as ligature
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
		And the advice should contain no other advice items

Scenario: US character reported as ligature

	Given the baseline input is for The United States with US as ligature
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
		And the advice should contain no other advice items

Scenario: EOT character reported as ligature

	Given the baseline input is for The United States with EOT as ligature
	When the baseline input to submitted to an agnostic calibrator
	    And advice is generated from the calculated system capabilities
	Then the advice should contain an advice item for ReadsInvariantCharactersReliably
		And the advice should contain no other advice items