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
    
    for (j in 1:5){
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

# plot Friends
init10 <- fromJSON(file = "./init10.json")
out10 <- fromJSON(file = "./out10.json")
plot_json(init10)
plot_json(out10)

init_T4_100 <- fromJSON(file = "./init_T4_100.json")
out_T4_100 <- fromJSON(file = "./out_T4_100.json")
plot_json(init_T4_100)
plot_json(out_T4_100)

init10s <- fromJSON(file = "./init10s.json")
out10s <- fromJSON(file = "./out10s.json")
plot_json(init10s)
plot_json(out10s)


init5 <- fromJSON(file = "./init5.json")
out5 <- fromJSON(file = "./out5.json")
plot_json(init5)
plot_json(out5)

