export class ApiResult {}

export class ApiQueryCollectionResult<T> extends ApiResult {
    constructor(data: T[], take: number, skip: number, total: number) {
        super()
        this.data = data
        this.skip = skip
        this.take = take
        this.total = total
    }

    public data: T[]

    public take: number

    public skip: number

    public total: number
}
