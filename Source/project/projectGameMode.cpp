// Copyright Epic Games, Inc. All Rights Reserved.

#include "projectGameMode.h"
#include "projectCharacter.h"
#include "UObject/ConstructorHelpers.h"

AprojectGameMode::AprojectGameMode()
{
	// set default pawn class to our Blueprinted character
	static ConstructorHelpers::FClassFinder<APawn> PlayerPawnBPClass(TEXT("/Game/ThirdPersonCPP/Blueprints/ThirdPersonCharacter"));
	if (PlayerPawnBPClass.Class != NULL)
	{
		DefaultPawnClass = PlayerPawnBPClass.Class;
	}
}
