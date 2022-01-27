setwd("~/Projects/Traject4/R_files")
library("rjson")

init <- fromJSON(file = "./init.json")
out <- fromJSON(file = "./out.json")

plot_json <- function(data){
  plot(range(-5,5),
       range(-5,5),
       xlab = "x",
       ylab = "y",
       type="n")
  
  for (i in 1:10){
    # print(init[[i]])
    traj <- data[[i]]$Trajectories
    
    for (j in 1:4){
      xs <- traj[[j]]$X
      ys <- traj[[j]]$Y
      
      for (z in 1:19){
        x0 <-  xs[z]
        y0 <- ys[z]
        x1 <- xs[z+1]
        y1 <- ys[z+1]
        
        segments(x0 = x0,
                 y0 = y0,
                 x1 = x1,
                 y1 = y1,
                 col = "black")
      }
    }
  }
}

plot_json(init)

plot_json(out)

