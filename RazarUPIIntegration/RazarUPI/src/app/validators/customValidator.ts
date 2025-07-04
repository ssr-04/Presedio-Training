import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

// Custom validator to allow only specific UPI IDs
export function allowedUpiIdValidator(allowedIds: string[]): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const isAllowed = allowedIds.includes(control.value);
    return isAllowed ? null : { forbiddenUpiId: { value: control.value } };
  };
}