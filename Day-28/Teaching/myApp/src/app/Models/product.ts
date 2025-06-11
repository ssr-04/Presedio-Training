export class ProductModel
{
    // id:number = 0;
    // constructor(pid:number)
    // {
    //     this.id = pid;
    // }

    constructor(
        public id:number=0, 
        public title:string="", 
        public price:number=0,
        public thumbnail:string="",
        public description:string=""

    )
    {

    }
}