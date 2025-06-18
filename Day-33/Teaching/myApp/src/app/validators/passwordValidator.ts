import { AbstractControl, ValidationErrors, ValidatorFn } from "@angular/forms";


export const PasswordValidator = ():ValidatorFn => {

    return(control:AbstractControl):ValidationErrors|null => {
        const value = control?.value as string;
        if (
            value &&
            value.length >= 6 &&
            value.split('').some(char => /[A-Z]/.test(char)) &&
            value.split('').some(char => /[a-z]/.test(char)) &&
            value.split('').some(char => /[0-9]/.test(char))
        )
        {
            return null;
        }

        return {'lessSecure': "Password isn't strong enough"};
    }
}