import {ValidatorFn, ValidationErrors, AbstractControl} from '@angular/forms'

export const bannedWordsValidator = (bannedWords: string[]): ValidatorFn => {
    return (control: AbstractControl): ValidationErrors | null => {
        const value = control.value?.toLowerCase().trim();
        if(value && bannedWords.includes(value)){
            return {bannedWord: true};
        }
        return null;
    }
}