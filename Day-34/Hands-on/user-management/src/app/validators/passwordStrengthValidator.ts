import {ValidatorFn, ValidationErrors, AbstractControl} from '@angular/forms'

export const passwordStrengthValidator = (): ValidatorFn => {
    return (control: AbstractControl): ValidationErrors | null => {
        const value = control.value;
        if(!value){
            return null;
        }

        const hasNumber = /[0-9]/.test(value);
        const hasSymbol = /[!@#$%^&*():{}|<>?.,]/.test(value);
        const hasMinLength = value.length >= 6;

        const passwordValid = hasNumber && hasSymbol && hasMinLength;

        if(!passwordValid) {
            return {
                passwordStrength : {
                    hasNumber,
                    hasSymbol,
                    hasMinLength
                }
            };
        }

        return null;
    }
}