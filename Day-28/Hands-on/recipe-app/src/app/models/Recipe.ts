export class RecipeModel
{

    constructor(
        public name: string = "",
        public cuisine: string = "",
        public cookTimeMinutes: number = 0,
        public ingredients: string[] = [],
        public image:string = ""
    ){}
}