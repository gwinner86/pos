import Link from "next/link"
import { Button } from "@/components/ui/button"
import { RegisterModal } from "./register-modal"
import { ArrowRight, Sparkles } from "lucide-react"

export function Hero() {
    return (
        <section className="relative overflow-hidden bg-background pt-24 pb-24 md:pt-32 md:pb-32">
            <div className="container relative z-10 px-4 md:px-6">
                <div className="flex flex-col items-center gap-6 text-center">
                    <div className="inline-flex items-center rounded-full border px-3 py-1 text-sm font-medium bg-secondary text-secondary-foreground">
                        <span className="flex h-2 w-2 rounded-full bg-primary mr-2 animate-pulse"></span>
                        <span className="mr-2">New Release v2.0</span>
                        <span className="text-muted-foreground">|</span>
                        <Link href="#features" className="ml-2 flex items-center hover:underline">
                            Explore features <ArrowRight className="ml-1 h-3 w-3" />
                        </Link>
                    </div>

                    <div className="space-y-4 max-w-4xl">
                        <h1 className="text-4xl font-extrabold tracking-tight sm:text-5xl md:text-6xl lg:text-7xl">
                            The <span className="text-transparent bg-clip-text bg-gradient-to-r from-blue-600 to-cyan-500">Ultimate POS</span> Solution <br className="hidden sm:inline" />
                            for Modern Retail
                        </h1>
                        <p className="mx-auto max-w-[700px] text-muted-foreground md:text-xl leading-relaxed">
                            Empower your business with a seamless, cloud-based Point of Sale.
                            Manage inventory, track sales, and delight customers—all from one beautiful dashboard.
                        </p>
                    </div>

                    <div className="flex flex-col gap-3 min-[400px]:flex-row pt-4">
                        <RegisterModal>
                            <Button size="lg" className="h-12 px-8 text-base shadow-lg shadow-primary/20 hover:shadow-primary/40 transition-shadow">
                                <Sparkles className="mr-2 h-4 w-4" />
                                Start Free Trial
                            </Button>
                        </RegisterModal>
                        <Link href="/login">
                            <Button size="lg" variant="outline" className="h-12 px-8 text-base bg-background/50 backdrop-blur-sm">
                                Live Demo
                            </Button>
                        </Link>
                    </div>

                    {/* Dashboard Mockup Placeholder */}
                    <div className="mt-16 w-full max-w-5xl rounded-xl border bg-background/50 p-2 shadow-2xl backdrop-blur-sm">
                        <div className="overflow-hidden rounded-lg border bg-background shadow-sm">
                            <div className="flex h-8 items-center gap-1.5 border-b bg-muted/50 px-3">
                                <div className="h-3 w-3 rounded-full bg-red-400"></div>
                                <div className="h-3 w-3 rounded-full bg-yellow-400"></div>
                                <div className="h-3 w-3 rounded-full bg-green-400"></div>
                            </div>
                            <div className="relative aspect-video w-full bg-slate-50 dark:bg-slate-900 overflow-hidden group">
                                <div className="absolute inset-0 flex items-center justify-center">
                                    <div className="text-center space-y-2 opacity-50 group-hover:opacity-100 transition-opacity">
                                        <div className="text-4xl font-bold text-slate-200 dark:text-slate-800 tracking-widest">DASHBOARD UI</div>
                                        <p className="text-sm text-muted-foreground">(Interactive Preview Coming Soon)</p>
                                    </div>
                                    {/* Mock Content Lines */}
                                    <div className="absolute inset-x-8 top-12 bottom-8 grid grid-cols-12 gap-4 opacity-30">
                                        <div className="col-span-3 bg-primary/20 rounded-md h-full"></div>
                                        <div className="col-span-9 grid grid-rows-3 gap-4 h-full">
                                            <div className="row-span-1 grid grid-cols-3 gap-4">
                                                <div className="bg-blue-400/20 rounded-md"></div>
                                                <div className="bg-purple-400/20 rounded-md"></div>
                                                <div className="bg-orange-400/20 rounded-md"></div>
                                            </div>
                                            <div className="row-span-2 bg-slate-200 dark:bg-slate-800 rounded-md"></div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Background Effects */}
            <div className="absolute top-0 -left-10 h-[500px] w-[500px] rounded-full bg-purple-500/10 blur-[120px] mix-blend-multiply dark:mix-blend-normal" />
            <div className="absolute top-0 -right-10 h-[500px] w-[500px] rounded-full bg-blue-500/10 blur-[120px] mix-blend-multiply dark:mix-blend-normal" />
            <div className="absolute top-40 left-1/2 -translate-x-1/2 h-[500px] w-[800px] rounded-full bg-indigo-500/5 blur-[100px]" />
        </section>
    )
}
